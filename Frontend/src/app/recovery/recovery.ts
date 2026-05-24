import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../services/auth.service';
import { isPasswordValid, PASSWORD_POLICY_MESSAGE } from '../utils/password-policy';

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
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    // Sincronizar el estado con los parámetros de la URL
    this.route.queryParams.subscribe(params => {
      const requestedStep = Number(params['step']);
      if ([1, 2, 3].includes(requestedStep)) {
        this.step = requestedStep;
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

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.step = 2;

    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { step: 2, email: this.correo },
      queryParamsHandling: 'merge',
      replaceUrl: true
    });

    this.authService.solicitarRecuperacion(this.correo).subscribe({
      next: (response) => {
        this.isLoading = false;
        this.successMessage = '¡Código enviado! Revisa tu correo.';
        this.cdr.detectChanges();
      },
      error: (error) => {
        this.isLoading = false;
        this.step = 1;
        this.router.navigate([], {
          relativeTo: this.route,
          queryParams: { step: 1, email: this.correo },
          queryParamsHandling: 'merge',
          replaceUrl: true
        });
        this.errorMessage = error.error?.mensaje || 'No pudimos encontrar ese correo.';
        console.error('Error en recuperación:', error);
        this.cdr.detectChanges();
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
    this.successMessage = '';

    this.authService.validarCodigoRecuperacion(this.correo, this.codigo).subscribe({
      next: (response) => {
        this.isLoading = false;
        this.step = 3;
        this.cdr.detectChanges();
        this.router.navigate([], {
          relativeTo: this.route,
          queryParams: { step: 3, code: this.codigo },
          queryParamsHandling: 'merge',
          replaceUrl: true
        });
      },
      error: (error) => {
        console.error('Error de validación:', error);
        this.isLoading = false;
        this.errorMessage = error.error?.mensaje || 'El código es incorrecto o ha expirado.';
        this.cdr.detectChanges();
      }
    });
  }

  // Paso 3: Cambiar contraseña
  resetearPassword() {
    if (!isPasswordValid(this.nuevaPassword)) {
      this.errorMessage = PASSWORD_POLICY_MESSAGE;
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
        this.cdr.detectChanges();
        setTimeout(() => {
          this.router.navigate(['/login']);
        }, 3000);
      },
      error: (error) => {
        this.isLoading = false;
        this.errorMessage = error.error?.mensaje || 'Error al actualizar la contraseña. Inténtalo de nuevo.';
        this.cdr.detectChanges();
      }
    });
  }
}
