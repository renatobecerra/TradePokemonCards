import { describe, it, expect, beforeEach } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { TcgService } from './tcg.service';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';

describe('TcgService (Inventario)', () => {
  let service: TcgService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [TcgService]
    });
    service = TestBed.inject(TcgService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  it('debe crearse correctamente', () => {
    expect(service).toBeTruthy();
  });

  it('debe obtener el inventario de un usuario', () => {
    const mockData = [{ id: 1, carta: 'Pikachu' }];
    service.getInventario(1).subscribe(res => {
      expect(res).toEqual(mockData);
    });
    const req = httpMock.expectOne('http://localhost:5210/api/inventario/1');
    expect(req.request.method).toBe('GET');
    req.flush(mockData);
    httpMock.verify();
  });

  it('debe agregar una carta al inventario', () => {
    const mockData = { id: 1, carta: 'Charizard' };
    service.agregarCarta(mockData).subscribe(res => {
      expect(res.success).toBe(true);
    });
    const req = httpMock.expectOne('http://localhost:5210/api/inventario/agregar');
    expect(req.request.method).toBe('POST');
    req.flush({ success: true });
    httpMock.verify();
  });

  it('debe editar una carta en el inventario', () => {
    const mockData = { precio: 100 };
    service.editarCarta(1, mockData).subscribe(res => {
      expect(res.success).toBe(true);
    });
    const req = httpMock.expectOne('http://localhost:5210/api/inventario/editar/1');
    expect(req.request.method).toBe('PUT');
    req.flush({ success: true });
    httpMock.verify();
  });

  it('debe eliminar una carta del inventario', () => {
    service.eliminarCarta(1).subscribe(res => {
      expect(res.success).toBe(true);
    });
    const req = httpMock.expectOne('http://localhost:5210/api/inventario/eliminar/1');
    expect(req.request.method).toBe('DELETE');
    req.flush({ success: true });
    httpMock.verify();
  });
});
