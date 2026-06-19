import { Component, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router, RouterLinkActive } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'comp-nav-bar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './comp-nav-bar.html',
  styleUrls: ['./comp-nav-bar.css']
})
export class CompNavBarComponent implements OnInit {
  private authService = inject(AuthService);
  private router = inject(Router);

  public currentUser = signal<any>(null);
  public showProfileMenu = signal<boolean>(false);

  ngOnInit() {
    this.authService.currentUser$.subscribe(user => {
      this.currentUser.set(user);
    });
  }

  toggleProfileMenu(event: Event) {
    event.stopPropagation();
    this.showProfileMenu.set(!this.showProfileMenu());
  }

  setPresence(event: Event, estado: number) {
    event.stopPropagation();
    const user = this.currentUser();
    if (!user || !user.id) return;

    const estadoAnterior = user.estadoPresencia;
    this.currentUser.set({ ...user, estadoPresencia: estado });
    this.showProfileMenu.set(false);

    this.authService.cambiarPresencia(user.id, estado).subscribe({
      error: (err) => {
        console.error('Error al sincronizar estado:', err);
        this.currentUser.set({ ...user, estadoPresencia: estadoAnterior });
      }
    });
  }

  getPresenceColor(): string {
    const estado = this.currentUser()?.estadoPresencia;
    switch (estado) {
      case 1: return '#2ecc71';
      case 2: return '#e74c3c';
      case 0: return '#95afc0';
      default: return '#2ecc71';
    }
  }

  onLogout() {
    this.authService.logout();
    this.router.navigate(['/']);
  }
}
