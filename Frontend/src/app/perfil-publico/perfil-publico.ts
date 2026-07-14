import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { CompNavBarComponent } from '../comp-nav-bar/comp-nav-bar';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-perfil-publico',
  standalone: true,
  imports: [CommonModule, RouterLink, CompNavBarComponent],
  templateUrl: './perfil-publico.html',
  styleUrls: ['./perfil-publico.css']
})
export class PerfilPublicoComponent implements OnInit {
  usuario = signal<any>(null);
  catalogo = signal<any[]>([]);
  resenas = signal<any[]>([]);
  activeTab = signal<string>('catalogo');
  loading = signal<boolean>(true);
  error = signal<string | null>(null);
  currentUser = signal<any>(null);

  constructor(
    private route: ActivatedRoute,
    private http: HttpClient,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.authService.currentUser$.subscribe(user => {
      this.currentUser.set(user);
    });

    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id) {
        this.cargarDatos(parseInt(id, 10));
      } else {
        this.error.set('No se proporcionó un ID de usuario válido.');
        this.loading.set(false);
      }
    });
  }

  cargarDatos(idUsuario: number) {
    this.loading.set(true);
    this.error.set(null);

    this.http.get<any>(`http://localhost:5210/api/auth/publico/${idUsuario}`).subscribe({
      next: (user) => {
        this.usuario.set(user);

        this.http.get<any[]>(`http://localhost:5210/api/inventario/${idUsuario}`).subscribe({
          next: (cards) => this.catalogo.set(cards),
          error: () => this.catalogo.set([])
        });

        this.http.get<any[]>(`http://localhost:5210/api/Resena/usuario/${idUsuario}`).subscribe({
          next: (revs) => this.resenas.set(revs),
          error: () => this.resenas.set([])
        });

        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error al cargar perfil público:', err);
        this.error.set('El entrenador solicitado no existe o no pudo ser cargado.');
        this.loading.set(false);
      }
    });
  }

  getPresenceColor(estado: number | null | undefined): string {
    if (estado === 1) return '#2ecc71';
    if (estado === 2) return '#f1c40f';
    return '#95a5a6';
  }

  getPresenceLabel(estado: number | null | undefined): string {
    if (estado === 1) return 'CONECTADO';
    if (estado === 2) return 'AUSENTE';
    return 'DESCONECTADO';
  }

  getStarsArray(val: number | null | undefined): number[] {
    const score = val || 0;
    const rounded = Math.round(score);
    return Array(rounded).fill(0);
  }

  getEmptyStarsArray(val: number | null | undefined): number[] {
    const score = val || 0;
    const rounded = Math.round(score);
    return Array(5 - rounded).fill(0);
  }
}
