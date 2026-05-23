import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'http://localhost:5210/api/auth';
  private currentUserSubject = new BehaviorSubject<any>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {
    // Recuperar usuario de localStorage al iniciar
    const savedUser = localStorage.getItem('usuario');
    if (savedUser) {
      this.currentUserSubject.next(this.normalizarUsuario(JSON.parse(savedUser)));
    }
  }

  private normalizarUsuario(user: any) {
    if (!user) return null;
    return {
      id: user.id || user.ID_Usuarios || user.IdUsuarios,
      nombre: user.nombre || user.Nombre,
      apellido: user.apellido || user.Apellido,
      correo: user.correo || user.Correo,
      telefono: user.telefono || user.Telefono,
      rol: user.rol || user.Rol,
      foto: user.foto || user.ImgPerfil || user.IMG_Perfil,
      estadoPresencia: user.estadoPresencia ?? user.EstadoPresencia ?? 1,
      bio: user.bio || user.Bio || user.Descripcion
    };
  }

  get currentUserValue() {
    return this.currentUserSubject.value;
  }

  login(correo: string, contrasena: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/login`, {
      Correo: correo,
      Contraseña: contrasena
    }).pipe(
      tap((response: any) => {
        if (response.usuario) {
          const user = this.normalizarUsuario(response.usuario);
          localStorage.setItem('usuario', JSON.stringify(user));
          this.currentUserSubject.next(user);
        }
      })
    );
  }

  cambiarPresencia(usuarioId: number, nuevoEstado: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/cambiar-presencia`, {
      UsuarioId: usuarioId,
      NuevoEstado: nuevoEstado
    }).pipe(
      tap(() => {
        const currentUser = this.currentUserSubject.value;
        if (currentUser) {
          const updatedUser = { ...currentUser, estadoPresencia: nuevoEstado };
          localStorage.setItem('usuario', JSON.stringify(updatedUser));
          this.currentUserSubject.next(updatedUser);
        }
      })
    );
  }

  actualizarPerfil(datos: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/actualizar-perfil`, datos).pipe(
      tap((response: any) => {
        const currentUser = this.currentUserSubject.value;
        if (currentUser && response.usuario) {
          const updatedUser = this.normalizarUsuario(response.usuario);
          localStorage.setItem('usuario', JSON.stringify(updatedUser));
          this.currentUserSubject.next(updatedUser);
        }
      })
    );
  }

  cambiarPassword(datos: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/cambiar-password`, datos);
  }

  logout() {
    localStorage.removeItem('usuario');
    this.currentUserSubject.next(null);
  }

  register(usuario: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/registrar`, usuario);
  }

  verificar(correo: string, codigo: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/verificar`, { Correo: correo, Codigo: codigo });
  }

  solicitarRecuperacion(correo: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/solicitar-recuperacion`, { Correo: correo });
  }

  validarCodigoRecuperacion(correo: string, codigo: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/validar-codigo-recuperacion`, { Correo: correo, Codigo: codigo });
  }

  resetearPassword(datos: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/resetear-password`, {
      Correo: datos.correo,
      Codigo: datos.codigo,
      NuevaPassword: datos.nuevaPassword
    });
  }

  loginWithGoogle(credential: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/google`, JSON.stringify(credential), {
      headers: { 'Content-Type': 'application/json' }
    }).pipe(
      tap((response: any) => {
        if (response.usuario) {
          const user = this.normalizarUsuario(response.usuario);
          localStorage.setItem('usuario', JSON.stringify(user));
          this.currentUserSubject.next(user);
        }
      })
    );
  }
}
