import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5210/api/admin';

  getUsuarios(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/usuarios`);
  }

  cambiarEstadoUsuario(idUsuario: number, nuevoEstado: number): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/usuarios/estado/${idUsuario}`, nuevoEstado, {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  banearUsuario(idUsuario: number, dias: number, motivo: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/usuarios/banear/${idUsuario}`, {
      dias,
      motivo
    });
  }

  eliminarUsuario(idUsuario: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/usuarios/eliminar/${idUsuario}`);
  }

  getArticulosMercado(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/articulos`);
  }

  eliminarArticuloMercado(idInventarioUser: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/articulos/eliminar/${idInventarioUser}`);
  }

  cambiarRolUsuario(idUsuario: number, nuevoRol: string): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/usuarios/rol/${idUsuario}`, JSON.stringify(nuevoRol), {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  crearReporte(reporte: { IdUsuarioReportante: number, IdUsuarioReportado: number, Motivo: string }): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/reportes`, reporte);
  }

  getReportes(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/reportes`);
  }

  eliminarReporte(idReporte: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/reportes/${idReporte}`);
  }
}
