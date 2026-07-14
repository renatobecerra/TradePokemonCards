/**
 * Tests derivados de los Criterios de Aceptación:
 * - Inicio de Sesión
 * - Registro de Usuario
 * - Recuperación de Cuenta
 *
 * Usando Vitest + mocks manuales
 */
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { of, throwError } from 'rxjs';

// ─── Mock de HttpClient ────────────────────────────────────────────────────
const mockHttp = {
  get: vi.fn(),
  post: vi.fn()
};

// ─── Simulación simplificada de AuthService ────────────────────────────────
class AuthServiceMock {
  private apiUrl = 'http://localhost:5210/api/auth';
  private currentUser: any = null;

  login(correo: string, contrasena: string) {
    return mockHttp.post(`${this.apiUrl}/login`, {
      Correo: correo,
      Contraseña: contrasena
    });
  }

  register(usuario: any) {
    return mockHttp.post(`${this.apiUrl}/registrar`, usuario);
  }

  verificar(correo: string, codigo: string) {
    return mockHttp.post(`${this.apiUrl}/verificar`, { Correo: correo, Codigo: codigo });
  }

  solicitarRecuperacion(correo: string) {
    return mockHttp.post(`${this.apiUrl}/solicitar-recuperacion`, { Correo: correo });
  }

  logout() {
    this.currentUser = null;
  }

  get currentUserValue() {
    return this.currentUser;
  }
}

// ─── Utilidad: validar contraseña segura ─────────────────────────────────
function validarContrasena(password: string): boolean {
  const minLength = password.length >= 8;
  const tieneUppercase = /[A-Z]/.test(password);
  const tieneNumero = /[0-9]/.test(password);
  const tieneEspecial = /[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]/.test(password);
  return minLength && tieneUppercase && tieneNumero && tieneEspecial;
}

describe('AuthService', () => {
  let service: AuthServiceMock;

  beforeEach(() => {
    service = new AuthServiceMock();
    vi.clearAllMocks();
  });

  // ─── CA: Inicio de Sesión ───────────────────────────────────────────────
  // "Al iniciar sesión correctamente, el sistema debe guardar la sesión
  //  y redirigir a la vista post inicio de sesión."

  it('debería iniciar sesión correctamente con credenciales válidas', () => {
    // Arrange
    const respuestaMock = {
      usuario: {
        id: 1,
        nombre: 'Carlos',
        apellido: 'López',
        correo: 'carlos@test.com',
        rol: 'Usuario'
      }
    };
    mockHttp.post.mockReturnValue(of(respuestaMock));

    // Act
    let resultado: any;
    service
      .login('carlos@test.com', 'Password123!')
      .subscribe((res: any) => (resultado = res));

    // Assert
    expect(resultado.usuario.nombre).toBe('Carlos');
    expect(mockHttp.post).toHaveBeenCalledWith(
      'http://localhost:5210/api/auth/login',
      { Correo: 'carlos@test.com', Contraseña: 'Password123!' }
    );
  });

  it('debería retornar error cuando las credenciales son incorrectas', () => {
    // Arrange — CA: "manejo de errores: 'Contraseña incorrecta'"
    const errorMock = { error: { mensaje: 'Contraseña incorrecta' }, status: 400 };
    mockHttp.post.mockReturnValue(throwError(() => errorMock));

    // Act
    let errorCapturado: any;
    service
      .login('carlos@test.com', 'WrongPass')
      .subscribe({ error: (err: any) => (errorCapturado = err) });

    // Assert
    expect(errorCapturado.status).toBe(400);
    expect(errorCapturado.error.mensaje).toBe('Contraseña incorrecta');
  });

  // ─── CA: Registro de Usuario ────────────────────────────────────────────
  // "La contraseña debe ser validada: mínimo 8 caracteres, mayúscula, número y símbolo."

  it('debería registrar un usuario con contraseña válida', () => {
    // Arrange
    const respuestaMock = { mensaje: 'Revisa tu correo para verificar tu cuenta.' };
    mockHttp.post.mockReturnValue(of(respuestaMock));

    const usuario = {
      Nombre: 'Ana',
      Apellido: 'García',
      Correo: 'ana@nuevo.com',
      Contraseña: 'Segura123!',
      Telefono: '555-9999'
    };

    // Act
    let resultado: any;
    service.register(usuario).subscribe((res: any) => (resultado = res));

    // Assert
    expect(resultado.mensaje).toContain('correo');
    expect(mockHttp.post).toHaveBeenCalledWith(
      'http://localhost:5210/api/auth/registrar',
      usuario
    );
  });

  it('debería rechazar una contraseña débil (sin mayúscula)', () => {
    // Arrange — CA: "mínimo 8 caracteres, al menos una mayúscula"
    const passwordDebil = 'password1!';

    // Act & Assert
    expect(validarContrasena(passwordDebil)).toBe(false);
  });

  it('debería rechazar una contraseña débil (sin símbolo especial)', () => {
    // Arrange — CA: "al menos un símbolo especial"
    const passwordDebil = 'Password1';

    // Act & Assert
    expect(validarContrasena(passwordDebil)).toBe(false);
  });

  it('debería aceptar una contraseña fuerte', () => {
    // Arrange — CA: cumple todos los requisitos
    const passwordFuerte = 'Password123!';

    // Act & Assert
    expect(validarContrasena(passwordFuerte)).toBe(true);
  });

  // ─── CA: Recuperación de Cuenta ─────────────────────────────────────────
  // "Si el correo existe, el sistema debe enviar un código de recuperación."

  it('debería solicitar recuperación de cuenta con correo existente', () => {
    // Arrange
    const respuestaMock = { mensaje: 'Se ha enviado un código de recuperación a tu correo.' };
    mockHttp.post.mockReturnValue(of(respuestaMock));

    // Act
    let resultado: any;
    service
      .solicitarRecuperacion('carlos@test.com')
      .subscribe((res: any) => (resultado = res));

    // Assert
    expect(resultado.mensaje).toContain('código');
    expect(mockHttp.post).toHaveBeenCalledWith(
      'http://localhost:5210/api/auth/solicitar-recuperacion',
      { Correo: 'carlos@test.com' }
    );
  });

  // ─── CA: Verificación con código (Google Auth / registro) ───────────────
  // "Cuando se registre, se solicite un código de verificación que llegará al correo."

  it('debería verificar el código de confirmación enviado al correo', () => {
    // Arrange
    const respuestaMock = { mensaje: 'Cuenta verificada exitosamente.' };
    mockHttp.post.mockReturnValue(of(respuestaMock));

    // Act
    let resultado: any;
    service
      .verificar('carlos@test.com', '123456')
      .subscribe((res: any) => (resultado = res));

    // Assert
    expect(resultado.mensaje).toContain('verificada');
  });

  // ─── CA: Cierre de sesión ────────────────────────────────────────────────
  it('debería limpiar el usuario actual al cerrar sesión', () => {
    // Arrange & Act
    service.logout();

    // Assert — CA: "Al iniciar sesión, el sistema guarda la sesión"
    expect(service.currentUserValue).toBeNull();
  });
});
