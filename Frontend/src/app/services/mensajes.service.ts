import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class MensajesService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5210/api/mensajes';

  private unreadChatsSubject = new BehaviorSubject<number>(0);
  public unreadChatsCount$ = this.unreadChatsSubject.asObservable();

  getConversaciones(usuarioId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/conversaciones/${usuarioId}`).pipe(
      tap((convs) => {
        const unreadCount = convs.filter(c => c.noLeidos > 0).length;
        this.unreadChatsSubject.next(unreadCount);
      })
    );
  }

  getHistorial(usuarioId: number, contactoId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/historial/${usuarioId}/${contactoId}`);
  }

  enviarMensaje(idRemitente: number, idDestinatario: number, texto: string, idItem?: number | null): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/enviar`, {
      idRemitente,
      idDestinatario,
      texto,
      idItem: idItem || null
    });
  }

  getUsuarioDetalle(usuarioId: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/usuario/${usuarioId}`);
  }

  deleteConversacion(usuarioId: number, contactoId: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/conversacion/${usuarioId}/${contactoId}`);
  }

  actualizarContadorPendientes(usuarioId: number): void {
    this.http.get<any[]>(`${this.apiUrl}/conversaciones/${usuarioId}`).subscribe({
      next: (convs) => {
        const unreadCount = convs.filter(c => c.noLeidos > 0).length;
        this.unreadChatsSubject.next(unreadCount);
      },
      error: (err) => console.error('Error al actualizar contador de pendientes:', err)
    });
  }
}
