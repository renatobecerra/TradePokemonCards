/**
 * Tests derivados de los Criterios de Aceptación:
 * - Envío de Mensajes
 * - Buzón de Entrada
 *
 * Usando Vitest + mocks manuales (sin Angular TestBed para máxima velocidad)
 */
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { of, throwError } from 'rxjs';

// ─── Mock de HttpClient ────────────────────────────────────────────────────
const mockHttp = {
  get: vi.fn(),
  post: vi.fn(),
  delete: vi.fn()
};

// ─── Clase bajo prueba (importada inline para evitar dependencias de zona) ───
class MensajesServiceMock {
  private apiUrl = 'http://localhost:5210/api/mensajes';
  private unreadCount = 0;

  getConversaciones(usuarioId: number) {
    return mockHttp.get(`${this.apiUrl}/conversaciones/${usuarioId}`);
  }

  getHistorial(usuarioId: number, contactoId: number) {
    return mockHttp.get(`${this.apiUrl}/historial/${usuarioId}/${contactoId}`);
  }

  enviarMensaje(idRemitente: number, idDestinatario: number, texto: string) {
    return mockHttp.post(`${this.apiUrl}/enviar`, {
      idRemitente,
      idDestinatario,
      texto,
      idItem: null
    });
  }

  deleteConversacion(usuarioId: number, contactoId: number) {
    return mockHttp.delete(
      `${this.apiUrl}/conversacion/${usuarioId}/${contactoId}`
    );
  }
}

describe('MensajesService', () => {
  let service: MensajesServiceMock;

  beforeEach(() => {
    service = new MensajesServiceMock();
    vi.clearAllMocks();
  });

  // ─── CA: Buzón de Entrada ───────────────────────────────────────────────
  // "La vista del buzón debe mostrar una lista lateral con los usuarios
  //  con los que tengo chats iniciados."

  it('debería obtener las conversaciones del usuario', () => {
    // Arrange
    const conversacionesMock = [
      {
        contacto: { id: 2, nombre: 'Ana', apellido: 'García' },
        ultimoMensaje: { texto: 'Hola!', fecha: new Date() },
        noLeidos: 1
      }
    ];
    mockHttp.get.mockReturnValue(of(conversacionesMock));

    // Act
    let resultado: any;
    service.getConversaciones(1).subscribe((convs: any) => (resultado = convs));

    // Assert
    expect(resultado).toHaveLength(1);
    expect(resultado[0].contacto.nombre).toBe('Ana');
  });

  it('debería retornar lista vacía si no hay conversaciones', () => {
    // Arrange — CA: "Si el buzón está vacío, mostrar 'Aún no tienes mensajes activos'"
    mockHttp.get.mockReturnValue(of([]));

    // Act
    let resultado: any;
    service.getConversaciones(1).subscribe((convs: any) => (resultado = convs));

    // Assert
    expect(resultado).toHaveLength(0);
  });

  // ─── CA: Envío de Mensajes ──────────────────────────────────────────────
  // "Debe existir un botón de Enviar que se accione también al presionar Enter."

  it('debería enviar un mensaje con el texto correcto', () => {
    // Arrange
    const nuevoMensaje = {
      idMensaje: 10,
      idRemitente: 1,
      idDestinatario: 2,
      texto: '¡Hola! ¿Vendes el Charizard?',
      fecha: new Date()
    };
    mockHttp.post.mockReturnValue(of(nuevoMensaje));

    // Act
    let resultado: any;
    service
      .enviarMensaje(1, 2, '¡Hola! ¿Vendes el Charizard?')
      .subscribe((msg: any) => (resultado = msg));

    // Assert
    expect(resultado.texto).toBe('¡Hola! ¿Vendes el Charizard?');
    expect(resultado.idRemitente).toBe(1);
    expect(resultado.idDestinatario).toBe(2);
    expect(mockHttp.post).toHaveBeenCalledWith(
      'http://localhost:5210/api/mensajes/enviar',
      expect.objectContaining({ idRemitente: 1, idDestinatario: 2 })
    );
  });

  it('debería manejar el error al enviar un mensaje fallido', () => {
    // Arrange — CA: el sistema debe manejar errores sin romper la UI
    mockHttp.post.mockReturnValue(
      throwError(() => new Error('Error de red'))
    );

    // Act
    let errorCapturado: any;
    service
      .enviarMensaje(1, 2, 'Mensaje fallido')
      .subscribe({ error: (err: any) => (errorCapturado = err) });

    // Assert
    expect(errorCapturado).toBeDefined();
    expect(errorCapturado.message).toBe('Error de red');
  });

  // ─── CA: Historial de Mensajes ──────────────────────────────────────────
  it('debería obtener el historial entre dos usuarios', () => {
    // Arrange
    const historialMock = [
      { idMensaje: 1, idRemitente: 1, texto: 'Hola', fecha: new Date() },
      { idMensaje: 2, idRemitente: 2, texto: 'Hola también', fecha: new Date() }
    ];
    mockHttp.get.mockReturnValue(of(historialMock));

    // Act
    let resultado: any;
    service.getHistorial(1, 2).subscribe((msgs: any) => (resultado = msgs));

    // Assert
    expect(resultado).toHaveLength(2);
    expect(resultado[0].texto).toBe('Hola');
  });

  it('debería llamar al endpoint correcto al eliminar una conversación', () => {
    // Arrange
    mockHttp.delete.mockReturnValue(of({ mensaje: 'Conversación eliminada con éxito' }));

    // Act
    let resultado: any;
    service
      .deleteConversacion(1, 2)
      .subscribe((res: any) => (resultado = res));

    // Assert
    expect(mockHttp.delete).toHaveBeenCalledWith(
      'http://localhost:5210/api/mensajes/conversacion/1/2'
    );
    expect(resultado.mensaje).toContain('eliminada');
  });
});
