import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../services/auth.service';
import { isPasswordValid, PASSWORD_POLICY_MESSAGE } from '../utils/password-policy';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './register.html',
  styleUrls: ['./register.css']
})
export class RegisterComponent {
  showPassword = false;
  showConfirmPassword = false;

  nombre: string = '';
  apellido: string = '';
  correo: string = '';
  telefono: string = '';
  contrasena: string = '';
  confirmarContrasena: string = '';
  errorMessage: string = '';

  constructor(
    private authService: AuthService, 
    private router: Router
  ) {}

  togglePasswordVisibility() {
    this.showPassword = !this.showPassword;
  }

  toggleConfirmPasswordVisibility() {
    this.showConfirmPassword = !this.showConfirmPassword;
  }

  onSubmit() {
    if (!this.nombre || !this.apellido || !this.correo || !this.contrasena || !this.confirmarContrasena) {
      this.errorMessage = 'Por favor, completa todos los campos obligatorios.';
      return;
    }

    if (!isPasswordValid(this.contrasena)) {
      this.errorMessage = PASSWORD_POLICY_MESSAGE;
      return;
    }

    if (this.contrasena !== this.confirmarContrasena) {
      this.errorMessage = 'Las contraseñas no coinciden.';
      return;
    }

    if (this.telefono && this.telefono.length !== 9) {
      this.errorMessage = 'El teléfono debe tener exactamente 9 dígitos.';
      return;
    }

    const nuevoUsuario = {
      nombre: this.nombre,
      apellido: this.apellido,
      correo: this.correo,
      telefono: this.telefono ? '+56' + this.telefono : null,
      contraseña: this.contrasena
    };

    this.authService.register(nuevoUsuario).subscribe({
      next: (response) => {
        localStorage.setItem('pending_verification_email', this.correo);
        this.router.navigate(['/verify'], { queryParams: { email: this.correo } });
      },
      error: (error) => {
        console.error('Error en registro:', error);
        this.errorMessage = error.error?.mensaje || 'Error al registrarse. Inténtalo de nuevo.';
      }
    });
  }
}
