import { describe, it, expect, vi, beforeEach } from 'vitest';
import { of } from 'rxjs';
import { TestBed } from '@angular/core/testing';
import { TcgService } from './tcg.service';
import { HttpClient } from '@angular/common/http';

const mockHttp = {
  get: vi.fn(),
  post: vi.fn(),
  put: vi.fn(),
  delete: vi.fn()
};

describe('TcgService (Inventario)', () => {
  let service: TcgService;

  beforeEach(() => {
    vi.clearAllMocks();
    TestBed.configureTestingModule({
      providers: [
        TcgService,
        { provide: HttpClient, useValue: mockHttp }
      ]
    });
    service = TestBed.inject(TcgService);
  });

  it('debe crearse correctamente', () => {
    expect(service).toBeTruthy();
  });

  it('debe obtener el inventario de un usuario', () => {
    const mockData = [{ id: 1, carta: 'Pikachu' }];
    mockHttp.get.mockReturnValue(of(mockData));

    service.getInventario(1).subscribe(res => {
      expect(res).toEqual(mockData);
    });
    expect(mockHttp.get).toHaveBeenCalledWith('http://localhost:5210/api/inventario/1');
  });

  it('debe agregar una carta al inventario', () => {
    const mockData = { id: 1, carta: 'Charizard' };
    mockHttp.post.mockReturnValue(of({ success: true }));

    service.agregarCarta(mockData).subscribe(res => {
      expect(res.success).toBe(true);
    });
    expect(mockHttp.post).toHaveBeenCalledWith('http://localhost:5210/api/inventario/agregar', mockData);
  });

  it('debe editar una carta en el inventario', () => {
    const mockData = { precio: 100 };
    mockHttp.put.mockReturnValue(of({ success: true }));

    service.editarCarta(1, mockData).subscribe(res => {
      expect(res.success).toBe(true);
    });
    expect(mockHttp.put).toHaveBeenCalledWith('http://localhost:5210/api/inventario/editar/1', mockData);
  });

  it('debe eliminar una carta del inventario', () => {
    mockHttp.delete.mockReturnValue(of({ success: true }));

    service.eliminarCarta(1).subscribe(res => {
      expect(res.success).toBe(true);
    });
    expect(mockHttp.delete).toHaveBeenCalledWith('http://localhost:5210/api/inventario/eliminar/1');
  });
});
