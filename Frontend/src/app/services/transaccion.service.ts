import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface ProponerTratoDto {
  idVendedor: number;
  idComprador: number;
  idInventarioUser: number;
  precio: number | null;
}

@Injectable({
  providedIn: 'root'
})
export class TransaccionService {
  private apiUrl = 'http://localhost:5210/api/Transaccion';

  constructor(private http: HttpClient) {}

  proponerTrato(dto: ProponerTratoDto): Observable<any> {
    return this.http.post(`${this.apiUrl}/proponer`, dto);
  }

  confirmarTrato(dto: ProponerTratoDto): Observable<any> {
    return this.http.post(`${this.apiUrl}/confirmar`, dto);
  }

  verificarTransaccion(idVendedor: number, idComprador: number): Observable<{ permitida: boolean }> {
    return this.http.get<{ permitida: boolean }>(`${this.apiUrl}/verificar?idVendedor=${idVendedor}&idComprador=${idComprador}`);
  }
}
