import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../services/auth.service';
import { TcgService } from '../services/tcg.service';
import { CompNavBarComponent } from '../comp-nav-bar/comp-nav-bar';

@Component({
  selector: 'app-explorar',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, CompNavBarComponent],
  templateUrl: './explorar.html',
  styleUrls: ['./explorar.css']
})
export class ExplorarComponent implements OnInit {
  private authService = inject(AuthService);
  private tcgService = inject(TcgService);
  private router = inject(Router);

  public cartasTcg = signal<any[]>([]);
  public currentUser = signal<any>(null);

  // Filtros
  public searchNombre = '';
  public selectedRareza = '';
  public selectedSet = '';
  public rarities = signal<string[]>([]);
  public sets = signal<any[]>([]);
  public paginaActual = signal<number>(1);

  // Wishlist
  public wishlistMap = signal<Map<string, number>>(new Map());
  public wishlistedCardIds = signal<Set<string>>(new Set());

  // Notificaciones
  public toastMessage = signal<string | null>(null);

  ngOnInit() {
    this.authService.currentUser$.subscribe(user => {
      if (!user) {
        this.router.navigate(['/login']);
      } else {
        this.currentUser.set(user);
        this.cargarWishlist(user.id);
      }
    });

    this.tcgService.getCartas(undefined, undefined, undefined, this.paginaActual()).subscribe({
      next: (data) => this.cartasTcg.set(data),
      error: (err) => console.error('Error cargando TCGdex', err)
    });

    this.cargarFiltrosAuxiliares();
  }

  cargarFiltrosAuxiliares() {
    this.tcgService.getRarities().subscribe({
      next: (data) => this.rarities.set(data),
      error: (err) => console.error('Error al cargar rarezas', err)
    });

    this.tcgService.getSets().subscribe({
      next: (data) => this.sets.set(data),
      error: (err) => console.error('Error al cargar sets', err)
    });
  }

  cargarWishlist(userId: number) {
    this.tcgService.getWishlist(userId).subscribe({
      next: (list) => {
        const ids = new Set<string>();
        const map = new Map<string, number>();
        list.forEach(item => {
          const tgcId = item.idTgc || item.IdTgc;
          const itemId = item.idItem || item.IdItem;
          if (tgcId && itemId) {
            ids.add(tgcId);
            map.set(tgcId, itemId);
          }
        });
        this.wishlistedCardIds.set(ids);
        this.wishlistMap.set(map);
      },
      error: (err) => console.error('Error al cargar deseados', err)
    });
  }

  aplicarFiltros() {
    this.paginaActual.set(1);
    this.tcgService.getCartas(this.searchNombre, this.selectedRareza, this.selectedSet, this.paginaActual()).subscribe({
      next: (data) => this.cartasTcg.set(data),
      error: (err) => console.error('Error al filtrar cartas', err)
    });
  }

  verMas() {
    this.paginaActual.update(p => p + 1);
    this.tcgService.getCartas(this.searchNombre, this.selectedRareza, this.selectedSet, this.paginaActual()).subscribe({
      next: (nuevasCartas) => {
        this.cartasTcg.update(cartasActuales => [...cartasActuales, ...nuevasCartas]);
      },
      error: (err) => console.error('Error al cargar más cartas', err)
    });
  }

  limpiarFiltros() {
    this.searchNombre = '';
    this.selectedRareza = '';
    this.selectedSet = '';
    this.aplicarFiltros();
  }

  isWishlisted(cardId: string): boolean {
    return this.wishlistedCardIds().has(cardId);
  }

  toggleDeseados(card: any, event: Event) {
    event.stopPropagation();
    const user = this.currentUser();
    if (!user) return;

    const cardId = card.id;

    if (this.isWishlisted(cardId)) {
      const itemId = this.wishlistMap().get(cardId);
      if (itemId) {
        this.tcgService.eliminarDeWishlist(user.id, itemId).subscribe({
          next: () => {
            this.wishlistedCardIds.update(ids => {
              const copy = new Set(ids);
              copy.delete(cardId);
              return copy;
            });
            this.wishlistMap.update(map => {
              const copy = new Map(map);
              copy.delete(cardId);
              return copy;
            });
            this.mostrarNotificacion('Carta removida de tus deseados');
          },
          error: (err) => console.error('Error al eliminar de deseados', err)
        });
      }
    } else {
      const precioApi = card.pricing?.tcgplayer?.market || card.pricing?.cardmarket?.avg || null;
      const precioCLP = precioApi ? Math.round(precioApi * 950) : null;

      const datos = {
        idUsuario: user.id,
        idTgc: cardId,
        nombre: card.name,
        rarity: card.rarity || 'Normal',
        edicion: card.set?.name || 'Desconocido',
        imgLink: card.image,
        precio: precioCLP
      };

      this.tcgService.agregarAWishlist(datos).subscribe({
        next: (res) => {
          const newItemId = res.idItem;
          this.wishlistedCardIds.update(ids => {
            const copy = new Set(ids);
            copy.add(cardId);
            return copy;
          });
          this.wishlistMap.update(map => {
            const copy = new Map(map);
            if (newItemId) {
              copy.set(cardId, newItemId);
            }
            return copy;
          });
          this.mostrarNotificacion('Carta guardada en tus deseados.');
        },
        error: (err) => {
          this.mostrarNotificacion(err.error?.mensaje || 'No se pudo guardar la carta.');
        }
      });
    }
  }

  mostrarNotificacion(msg: string) {
    this.toastMessage.set(msg);
    setTimeout(() => {
      this.toastMessage.set(null);
    }, 3000);
  }

  getPrecioReferencial(card: any): string {
    const apiPrice = card.pricing?.tcgplayer?.market || card.pricing?.cardmarket?.avg || null;
    if (apiPrice) {
      return `$${Math.round(apiPrice * 950).toLocaleString('es-CL')} CLP`;
    }
    return 'N/A';
  }

  getRareza(card: any): string {
    if (card.rarity) return card.rarity;
    if (this.selectedRareza) {
      // Intentar formatear la opción seleccionada
      return this.selectedRareza;
    }
    return 'Común';
  }

  getEdicion(card: any): string {
    if (card.set?.name) {
      return card.set.name;
    }
    
    // TCGdex asigna IDs de cartas con formato "setId-cardNum" (ej: "xy1-1" o "base1-1")
    const cardId = card.id;
    if (cardId && cardId.includes('-')) {
      const parts = cardId.split('-');
      parts.pop(); // remover el número de la carta
      const setId = parts.join('-'); // volver a unir en caso de set IDs con guiones
      
      const matchedSet = this.sets().find(s => s.id === setId);
      if (matchedSet) {
        return matchedSet.name;
      }
    }
    
    return 'Desconocido';
  }
}
