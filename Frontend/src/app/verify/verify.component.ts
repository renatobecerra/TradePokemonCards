import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-verify',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './verify.component.html',
  styleUrls: ['./verify.component.css']
})
export class VerifyComponent implements OnInit {
  correo: string = '';
  codigo: string = '';
  errorMessage: string = '';
  successMessage: string = '';
  isLoading: boolean = false;

  constructor(
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit() {
    // Intentar obtener el correo de los parámetros de la URL (si lo pasamos así)
    this.route.queryParams.subscribe(params => {
      if (params['email']) {
        this.correo = params['email'];
      } else {
        // Si no está en la URL, intentar recuperarlo de un registro previo en memoria/storage
        const savedEmail = localStorage.getItem('pending_verification_email');
        if (savedEmail) {
          this.correo = savedEmail;
        }
      }
    });
  }

  onSubmit() {
    if (!this.codigo || this.codigo.length !== 6) {
      this.errorMessage = 'Por favor, ingresa el código de 6 dígitos.';
      return;
    }

    if (!this.correo) {
      this.errorMessage = 'No se encontró el correo electrónico asociado. Por favor, intenta iniciar sesión.';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.authService.verificar(this.correo, this.codigo).subscribe({
      next: (response) => {
        this.isLoading = false;
        this.successMessage = '¡Cuenta verificada con éxito! Redirigiendo al login...';
        localStorage.removeItem('pending_verification_email');
        setTimeout(() => {
          this.router.navigate(['/login']);
        }, 3000);
      },
      error: (error) => {
        this.isLoading = false;
        this.errorMessage = error.error?.mensaje || 'El código es incorrecto o ha expirado.';
      }
    });
  }
}
