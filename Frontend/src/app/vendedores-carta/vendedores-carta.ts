import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TcgService } from '../services/tcg.service';
import { CompNavBarComponent } from '../comp-nav-bar/comp-nav-bar';
import { ResenaService, Resena } from '../services/resena.service';
import { AuthService } from '../services/auth.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-vendedores-carta',
  standalone: true,
  imports: [CommonModule, RouterLink, CompNavBarComponent, FormsModule],
  templateUrl: './vendedores-carta.html',
  styleUrls: ['./vendedores-carta.css']
})
export class VendedoresCartaComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private tcgService = inject(TcgService);
  private resenaService = inject(ResenaService);
  private authService = inject(AuthService);

  public idTgc = signal<string>('');
  public cardDetails = signal<any>(null);
  public sellers = signal<any[]>([]);
  public loading = signal<boolean>(true);
  public error = signal<string | null>(null);

  // Review Modal State
  public showReviewModal = signal<boolean>(false);
  public selectedSeller = signal<any>(null);
  public newReviewRating = signal<number>(5);
  public newReviewText = signal<string>('');
  public reviewSubmitting = signal<boolean>(false);
  public reviewError = signal<string | null>(null);
  public reviewSuccess = signal<string | null>(null);

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      const id = params.get('idTgc');
      if (id) {
        this.idTgc.set(id);
        this.cargarDatos(id);
      }
    });
  }

  cargarDatos(id: string) {
    this.loading.set(true);
    this.error.set(null);

    // 1. Cargar detalles oficiales de la carta
    this.tcgService.getDetallesCarta(id).subscribe({
      next: (details) => {
        console.log('Detalles de la carta cargados:', details);
        this.cardDetails.set(details);
      },
      error: (err) => {
        console.error('Error al cargar detalles de la carta', err);
        // Fallback básico para mostrar al menos la estructura
        this.cardDetails.set({
          name: 'Carta de TCGdex',
          rarity: 'Desconocida',
          id: id,
          image: `https://assets.tcgdex.net/en/${id.split('-')[0]}/${id}`
        });
      }
    });

    // 2. Cargar vendedores locales de nuestro backend
    this.tcgService.getVendedoresCarta(id).subscribe({
      next: (data) => {
        this.sellers.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error al cargar vendedores', err);
        this.error.set('No se pudo conectar con el servidor para obtener los vendedores.');
        this.loading.set(false);
      }
    });
  }

  getPresenceColor(estado: number): string {
    switch (estado) {
      case 1: return '#2ecc71'; // Conectado
      case 2: return '#e74c3c'; // No molestar
      case 0: return '#95afc0'; // Invisible
      default: return '#2ecc71';
    }
  }

  getPresenceLabel(estado: number): string {
    switch (estado) {
      case 1: return 'Conectado';
      case 2: return 'No molestar';
      case 0: return 'Desconectado';
      default: return 'Conectado';
    }
  }

  getStarsArray(rating: number): number[] {
    const r = Math.round(rating || 5);
    return Array(r).fill(0);
  }

  getPrecioReferencial(): string {
    const card = this.cardDetails();
    if (!card) return 'N/A';
    const apiPrice = card.pricing?.tcgplayer?.market || card.pricing?.cardmarket?.avg || null;
    if (apiPrice) {
      return `$${Math.round(apiPrice * 950).toLocaleString('es-CL')} CLP`;
    }
    return 'N/A';
  }
}
