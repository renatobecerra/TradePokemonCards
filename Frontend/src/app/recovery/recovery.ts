import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-recovery',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './recovery.html',
  styleUrls: ['./recovery.css']
})
export class RecoveryComponent implements OnInit {
  step: number = 1; // 1: Email, 2: Code, 3: New Password
  
  correo: string = '';
  codigo: string = '';
  nuevaPassword: string = '';
  confirmarPassword: string = '';
  
  showPassword = false;
  showConfirmPassword = false;

  errorMessage: string = '';
  successMessage: string = '';
  isLoading: boolean = false;

  constructor(
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit() {
    // Sincronizar el estado con los parámetros de la URL
    this.route.queryParams.subscribe(params => {
      if (params['step']) {
        this.step = parseInt(params['step']);
      }
      if (params['email']) {
        this.correo = params['email'];
      }
      if (params['code']) {
        this.codigo = params['code'];
      }
    });
  }

  togglePasswordVisibility() {
    this.showPassword = !this.showPassword;
  }

  toggleConfirmPasswordVisibility() {
    this.showConfirmPassword = !this.showConfirmPassword;
  }

  // Paso 1: Solicitar código
  solicitarCodigo() {
    if (!this.correo) {
      this.errorMessage = 'Por favor, ingresa tu correo electrónico.';
      return;
    }

    // --- CAMBIO OPTIMISTA ---
    // Pasamos al paso 2 AL TIRO para que no sientas que se pega
    this.step = 2; 
    this.isLoading = true; // El cargador se queda en el paso 2 de fondo
    this.errorMessage = '';
    this.successMessage = '';

    this.authService.solicitarRecuperacion(this.correo).subscribe({
      next: (response) => {
        this.isLoading = false;
        this.successMessage = '¡Código enviado! Revisa tu correo.';
        // Actualizamos la URL para que todo sea coherente
        this.router.navigate([], {
          relativeTo: this.route,
          queryParams: { step: 2, email: this.correo },
          queryParamsHandling: 'merge'
        });
      },
      error: (error) => {
        this.isLoading = false;
        // Si falló de verdad (ej: el correo no existe), lo devolvemos al paso 1 para que corrija
        this.step = 1;
        this.errorMessage = error.error?.mensaje || 'No pudimos encontrar ese correo.';
        console.error('Error en recuperación:', error);
      }
    });
  }

  // Paso 2: Validar código
  validarCodigo() {
    if (!this.codigo || this.codigo.length !== 6) {
      this.errorMessage = 'Ingresa el código de 6 dígitos que recibiste.';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.authService.validarCodigoRecuperacion(this.correo, this.codigo).subscribe({
      next: (response) => {
        this.isLoading = false;
        // Navegamos al paso 3
        this.step = 3;
        this.router.navigate([], {
          relativeTo: this.route,
          queryParams: { step: 3, code: this.codigo },
          queryParamsHandling: 'merge'
        });
      },
      error: (error) => {
        this.isLoading = false;
        this.errorMessage = error.error?.mensaje || 'El código es incorrecto o ha expirado.';
      }
    });
  }

  // Paso 3: Cambiar contraseña
  resetearPassword() {
    if (!this.nuevaPassword || this.nuevaPassword.length < 8) {
      this.errorMessage = 'La nueva contraseña debe tener al menos 8 caracteres.';
      return;
    }

    if (this.nuevaPassword !== this.confirmarPassword) {
      this.errorMessage = 'Las contraseñas no coinciden.';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    const datos = {
      correo: this.correo,
      codigo: this.codigo,
      nuevaPassword: this.nuevaPassword
    };

    this.authService.resetearPassword(datos).subscribe({
      next: (response) => {
        this.isLoading = false;
        this.successMessage = '¡Contraseña actualizada con éxito! Redirigiendo...';
        setTimeout(() => {
          this.router.navigate(['/login']);
        }, 3000);
      },
      error: (error) => {
        this.isLoading = false;
        this.errorMessage = error.error?.mensaje || 'Error al actualizar la contraseña. Inténtalo de nuevo.';
      }
    });
  }
}
