import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-explorar',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './explorar.html',
  styleUrls: ['./explorar.css']
})
export class ExplorarComponent implements OnInit {
  private authService = inject(AuthService);
  private router = inject(Router);

  public currentUser = signal<any>(null);
  public showProfileMenu = signal<boolean>(false);

  ngOnInit() {
    this.authService.currentUser$.subscribe(user => {
      if (!user) {
        this.router.navigate(['/login']);
      }
      this.currentUser.set(user);
    });
  }

  toggleProfileMenu(event: Event) {
    event.stopPropagation();
    this.showProfileMenu.set(!this.showProfileMenu());
  }

  onLogout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  setPresence(event: Event, estado: number) {
    event.stopPropagation();
    const user = this.currentUser();
    
    if (!user || !user.id) {
      console.error('No se pudo cambiar el estado: Usuario no cargado o sin ID');
      return;
    }

    // Guardamos el estado anterior por si falla la petición
    const estadoAnterior = user.estadoPresencia;

    // Actualización Optimista: Cambiamos la UI antes de esperar al servidor
    this.currentUser.set({ ...user, estadoPresencia: estado });
    this.showProfileMenu.set(false); // Cerramos el menú para una experiencia fluida

    this.authService.cambiarPresencia(user.id, estado).subscribe({
      next: () => {
        console.log(`Estado cambiado exitosamente a: ${estado}`);
      },
      error: (err) => {
        console.error('Error al sincronizar estado con el servidor:', err);
        // Revertimos al estado anterior en caso de error
        this.currentUser.set({ ...user, estadoPresencia: estadoAnterior });
        alert('No se pudo guardar tu estado en el servidor. Inténtalo de nuevo.');
      }
    });
  }

  getPresenceColor(): string {
    const estado = this.currentUser()?.estadoPresencia;
    switch (estado) {
      case 1: return '#2ecc71'; // Conectado
      case 2: return '#e74c3c'; // No Molestar
      case 0: return '#95afc0'; // Desconectado
      default: return '#2ecc71';
    }
  }
}
