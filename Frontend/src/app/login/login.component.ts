import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  showPassword = false;
  correo: string = '';
  contrasena: string = '';
  errorMessage: string = '';

  constructor(
    private authService: AuthService, 
    private router: Router
  ) {}

  togglePasswordVisibility() {
    this.showPassword = !this.showPassword;
  }

  onSubmit() {
    if (!this.correo || !this.contrasena) {
      this.errorMessage = 'Por favor, completa todos los campos.';
      return;
    }

    this.authService.login(this.correo, this.contrasena).subscribe({
      next: (response) => {
        console.log('Login exitoso:', response);
        // Aquí podrías guardar el usuario en localStorage si fuera necesario
        localStorage.setItem('usuario', JSON.stringify(response.usuario));
        this.router.navigate(['/explorar']); // Redirige al Marketplace
      },
      error: (error) => {
        console.error('Error en login:', error);
        
        // Si el servidor indica que falta verificar, redirigimos
        if (error.status === 401 && error.error?.requiereVerificacion) {
          localStorage.setItem('pending_verification_email', this.correo);
          this.router.navigate(['/verify'], { queryParams: { email: this.correo } });
          return;
        }

        this.errorMessage = error.error?.mensaje || 'Error al iniciar sesión. Inténtalo de nuevo.';
      }
    });
  }
}
