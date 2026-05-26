import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../services/auth.service';
import { isPasswordValid, PASSWORD_POLICY_MESSAGE } from '../utils/password-policy';

@Component({
  selector: 'app-perfil',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './perfil.html',
  styleUrls: ['./perfil.css']
})
export class PerfilComponent implements OnInit {
  private authService = inject(AuthService);
  private router = inject(Router);

  public isEditing = signal<boolean>(false);
  public isChangingPassword = signal<boolean>(false);
  
  public showActual = signal<boolean>(false);
  public showNueva = signal<boolean>(false);
  public showConfirmar = signal<boolean>(false);
  
  public editData = {
    nombre: '',
    apellido: '',
    telefono: '',
    bio: '',
    foto: ''
  };

  public passwordData = {
    actual: '',
    nueva: '',
    confirmar: ''
  };

  public passwordMessage = signal<{text: string, type: 'success' | 'error'} | null>(null);

  public currentUser = signal<any>(null);

  ngOnInit() {
    this.authService.currentUser$.subscribe(user => {
      if (!user) {
        this.router.navigate(['/login']);
        return;
      }
      this.currentUser.set(user);
      this.resetEditData(user);
    });
  }

  resetEditData(user: any) {
    this.editData = {
      nombre: user.nombre || '',
      apellido: user.apellido || '',
      telefono: user.telefono ? user.telefono.replace('+56', '') : '',
      bio: user.bio || '',
      foto: user.foto || ''
    };
  }

  onFileSelected(event: any) {
    const file: File = event.target.files[0];
    if (file) {
      const reader = new FileReader();
      reader.onload = (e: any) => {
        const base64Image = e.target.result;
        this.editData.foto = base64Image;
        this.currentUser.update(user => ({ ...user, foto: base64Image }));
      };
      reader.readAsDataURL(file);
    }
  }

  toggleEdit() {
    if (this.isEditing()) {
      this.resetEditData(this.currentUser());
    }
    this.isEditing.set(!this.isEditing());
    if (this.isEditing()) this.isChangingPassword.set(false);
  }

  togglePasswordChange() {
    this.isChangingPassword.set(!this.isChangingPassword());
    if (this.isChangingPassword()) {
      this.isEditing.set(false);
      this.passwordData = { actual: '', nueva: '', confirmar: '' };
      this.passwordMessage.set(null);
    }
  }

  onSave() {
    if (this.editData.telefono && this.editData.telefono.length !== 9) {
      alert('El teléfono debe tener exactamente 9 dígitos.');
      return;
    }

    const user = this.currentUser();
    const updatedData = {
      UsuarioId: user.id,
      Nombre: this.editData.nombre,
      Apellido: this.editData.apellido,
      Telefono: this.editData.telefono ? '+56' + this.editData.telefono : null,
      ImgPerfil: this.editData.foto,
      Bio: this.editData.bio
    };

    this.authService.actualizarPerfil(updatedData).subscribe({
      next: (response) => {
        this.isEditing.set(false);
      },
      error: (err) => {
        console.error('Error al actualizar perfil:', err);
        alert('Hubo un error al guardar los cambios.');
      }
    });
  }

  onUpdatePassword() {
    if (this.passwordData.nueva !== this.passwordData.confirmar) {
      this.passwordMessage.set({ text: 'Las contraseñas nuevas no coinciden.', type: 'error' });
      return;
    }

    if (!isPasswordValid(this.passwordData.nueva)) {
      this.passwordMessage.set({ text: PASSWORD_POLICY_MESSAGE, type: 'error' });
      return;
    }

    const payload = {
      usuarioId: this.currentUser().id,
      passwordActual: this.passwordData.actual,
      nuevaPassword: this.passwordData.nueva
    };

    this.authService.cambiarPassword(payload).subscribe({
      next: () => {
        this.passwordMessage.set({ text: 'Contraseña actualizada con éxito.', type: 'success' });
        setTimeout(() => this.togglePasswordChange(), 2000);
      },
      error: (err) => {
        this.passwordMessage.set({ 
          text: err.error?.mensaje || 'Error al cambiar la contraseña.', 
          type: 'error' 
        });
      }
    });
  }

  onLogout() {
    this.authService.logout();
    this.router.navigate(['/']);
  }
  }
