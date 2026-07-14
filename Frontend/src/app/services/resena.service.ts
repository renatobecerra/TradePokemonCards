import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Resena {
  resenaId?: number;
  idUsuarioResenador: number;
  idUsuarioResenado?: number | null;
  idItem?: number | null;
  calificacion: number;
  texto: string;
  fecha?: string;
  nombreResenador?: string;
  imgResenador?: string;
}

@Injectable({
  providedIn: 'root'
})
export class ResenaService {
  private apiUrl = 'http://localhost:5210/api/Resena'; // Adjust the port if needed

  constructor(private http: HttpClient) { }

  getResenasPorUsuario(idUsuario: number): Observable<Resena[]> {
    return this.http.get<Resena[]>(`${this.apiUrl}/usuario/${idUsuario}`);
  }

  getResenasPorCarta(idCarta: number): Observable<Resena[]> {
    return this.http.get<Resena[]>(`${this.apiUrl}/carta/${idCarta}`);
  }

  crearResena(resena: Resena): Observable<any> {
    return this.http.post<any>(this.apiUrl, resena);
  }
}
