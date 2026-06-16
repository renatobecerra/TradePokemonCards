import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class TcgService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5210/api/tcg'; // Puerto real detectado

  getCartas(nombre?: string, rareza?: string): Observable<any[]> {
    let url = `${this.apiUrl}/cartas`;
    const params = [];
    if (nombre) params.push(`nombre=${nombre}`);
    if (rareza) params.push(`rareza=${rareza}`);
    
    if (params.length > 0) {
      url += `?${params.join('&')}`;
    }
    return this.http.get<any[]>(url);
  }

  getDetallesCarta(id: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/cartas/${id}`);
  }

  // --- UTILIDADES DE FILTRO (Consumo directo de TCGdex para selects) ---
  getRarities(): Observable<string[]> {
    return this.http.get<string[]>('https://api.tcgdex.net/v2/es/rarities');
  }

  getSets(): Observable<any[]> {
    return this.http.get<any[]>('https://api.tcgdex.net/v2/es/sets');
  }

  // --- MÉTODOS DE INVENTARIO PROPIO ---
  getInventario(idUsuario: number): Observable<any[]> {
    return this.http.get<any[]>(`http://localhost:5210/api/inventario/${idUsuario}`);
  }

  agregarCarta(datos: any): Observable<any> {
    return this.http.post<any>(`http://localhost:5210/api/inventario/agregar`, datos);
  }

  editarCarta(idInventarioUser: number, datos: any): Observable<any> {
    return this.http.put<any>(`http://localhost:5210/api/inventario/editar/${idInventarioUser}`, datos);
  }

  eliminarCarta(idInventarioUser: number): Observable<any> {
    return this.http.delete<any>(`http://localhost:5210/api/inventario/eliminar/${idInventarioUser}`);
  }
}
