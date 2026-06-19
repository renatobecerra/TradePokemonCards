import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { TcgService } from '../services/tcg.service';
import { AuthService } from '../services/auth.service';
import { CompNavBarComponent } from '../comp-nav-bar/comp-nav-bar';

@Component({
  selector: 'app-guardados',
  standalone: true,
  imports: [CommonModule, RouterLink, CompNavBarComponent],
  templateUrl: './guardados.html',
  styleUrls: ['./guardados.css']
})
export class GuardadosComponent implements OnInit {
  private tcgService = inject(TcgService);
  private authService = inject(AuthService);

  public currentUser = signal<any>(null);
  public wishlist = signal<any[]>([]);
  public loading = signal<boolean>(true);

  ngOnInit() {
    this.authService.currentUser$.subscribe(user => {
      if (user) {
        this.currentUser.set(user);
        this.cargarWishlist();
      }
    });
  }

  cargarWishlist() {
    const user = this.currentUser();
    if (!user) return;

    this.loading.set(true);
    this.tcgService.getWishlist(user.id).subscribe({
      next: (data) => {
        this.wishlist.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error al cargar deseados', err);
        this.loading.set(false);
      }
    });
  }

  eliminarDeseado(idItem: number) {
    const user = this.currentUser();
    if (!user) return;

    this.tcgService.eliminarDeWishlist(user.id, idItem).subscribe({
      next: () => {
        this.wishlist.update(list => list.filter(item => item.idItem !== idItem));
      },
      error: (err) => console.error('Error al eliminar deseado', err)
    });
  }

  getPrecioReferencial(item: any): string {
    const val = item.precio !== undefined ? item.precio : item.Precio;
    if (val) {
      return `$${val.toLocaleString('es-CL')} CLP`;
    }
    return 'N/A';
  }
}
