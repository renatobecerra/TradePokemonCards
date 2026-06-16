import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../services/auth.service';
import { TcgService } from '../services/tcg.service';

@Component({
  selector: 'app-inventario',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule],
  templateUrl: './inventario.html',
  styleUrls: ['./inventario.css']
})
export class InventarioComponent implements OnInit {
  private authService = inject(AuthService);
  private tcgService = inject(TcgService);
  private router = inject(Router);
  private fb = inject(FormBuilder);

  public currentUser = signal<any>(null);
  public miInventario = signal<any[]>([]);
  public mostrarModal = signal<boolean>(false);
  public step = signal<number>(1); // 1: Catálogo, 2: Configurar Carta
  public cargando = signal<boolean>(false);
  public modoEdicion = signal<boolean>(false);
  public idEditando = signal<number | null>(null);

  // Datos del Catálogo
  public catalogoTcg = signal<any[]>([]);
  public rarities = signal<string[]>([]);
  public sets = signal<any[]>([]);
  public cardSeleccionada = signal<any>(null);

  // Filtros
  public filtroNombre = signal<string>('');
  public filtroRareza = signal<string>('');
  public filtroEdicion = signal<string>('');

  public cardForm: FormGroup;

  constructor() {
    this.cardForm = this.fb.group({
      estado: ['Excelente', [Validators.required]],
      precio: [null, [Validators.required, Validators.min(0)]]
    });
  }

  ngOnInit() {
    this.authService.currentUser$.subscribe(user => {
      if (!user) {
        this.router.navigate(['/login']);
        return;
      }
      this.currentUser.set(user);
      this.cargarInventario();
    });

    this.cargarFiltros();
    this.buscarEnCatalogo();
  }

  cargarInventario() {
    const user = this.currentUser();
    if (user) {
      this.tcgService.getInventario(user.id).subscribe({
        next: (data) => this.miInventario.set(data),
        error: (err) => console.error('Error al cargar inventario', err)
      });
    }
  }

  cargarFiltros() {
    this.tcgService.getRarities().subscribe(data => this.rarities.set(data));
    this.tcgService.getSets().subscribe(data => this.sets.set(data));
  }

  buscarEnCatalogo() {
    this.cargando.set(true);
    this.tcgService.getCartas(this.filtroNombre(), this.filtroRareza()).subscribe({
      next: (data) => {
        this.catalogoTcg.set(data);
        this.cargando.set(false);
      },
      error: () => this.cargando.set(false)
    });
  }

  abrirModal() {
    this.modoEdicion.set(false);
    this.idEditando.set(null);
    this.step.set(1);
    this.mostrarModal.set(true);
    this.buscarEnCatalogo();
  }

  abrirEditar(item: any) {
    this.modoEdicion.set(true);
    this.idEditando.set(item.idInventarioUser);
    this.cardSeleccionada.set({
      name: item.nombre,
      rarity: item.rareza,
      image: item.imgLink.replace('/high.jpg', '') 
    });
    this.cardForm.patchValue({
      estado: item.estado,
      precio: item.precio
    });
    this.step.set(2);
    this.mostrarModal.set(true);
  }

  cerrarModal() {
    this.mostrarModal.set(false);
    this.cardSeleccionada.set(null);
    this.modoEdicion.set(false);
    this.idEditando.set(null);
    this.cardForm.reset({ estado: 'Excelente' });
  }

  seleccionarCard(card: any) {
    this.cargando.set(true);
    this.tcgService.getDetallesCarta(card.id).subscribe({
      next: (detalles) => {
        this.cardSeleccionada.set(detalles);
        const precioApi = detalles.pricing?.tcgplayer?.market || detalles.pricing?.cardmarket?.avg;
        this.cardForm.patchValue({
          precio: precioApi || null
        });
        this.step.set(2);
        this.cargando.set(false);
      },
      error: (err) => {
        console.error('Error al obtener detalles', err);
        this.cardSeleccionada.set(card);
        this.step.set(2);
        this.cargando.set(false);
      }
    });
  }

  volverAlCatalogo() {
    this.step.set(1);
    this.cardSeleccionada.set(null);
  }

  eliminarCarta(id: number) {
    if (confirm('¿Estás seguro de que quieres eliminar esta carta de tu inventario?')) {
      this.tcgService.eliminarCarta(id).subscribe({
        next: () => this.cargarInventario(),
        error: (err) => console.error('Error al eliminar', err)
      });
    }
  }

  onSubmit() {
    if (this.cardForm.invalid || !this.cardSeleccionada()) return;

    const user = this.currentUser();
    const card = this.cardSeleccionada();
    
    this.cargando.set(true);
    const datos = {
      idUsuario: user.id,
      nombre: card.name,
      estado: this.cardForm.value.estado,
      rarity: card.rarity,
      edicion: 'TCGdex Set', 
      imgLink: card.image.includes('http') ? card.image : card.image + '/high.jpg',
      idTgc: card.id || null,
      precio: this.cardForm.value.precio
    };

    const request = this.modoEdicion() 
      ? this.tcgService.editarCarta(this.idEditando()!, datos)
      : this.tcgService.agregarCarta(datos);

    request.subscribe({
      next: () => {
        this.cargarInventario();
        this.cerrarModal();
        this.cargando.set(false);
      },
      error: (err) => {
        console.error('Error al procesar', err);
        this.cargando.set(false);
      }
    });
  }

  getPrecioReferencial(): number | null {
    const card = this.cardSeleccionada();
    return card?.pricing?.tcgplayer?.market || card?.pricing?.cardmarket?.avg || null;
  }
}
