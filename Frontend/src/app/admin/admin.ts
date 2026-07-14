import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../services/auth.service';
import { AdminService } from '../services/admin.service';
import { CompNavBarComponent } from '../comp-nav-bar/comp-nav-bar';

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [CommonModule, FormsModule, CompNavBarComponent],
  templateUrl: './admin.html',
  styleUrls: ['./admin.css']
})
export class AdminComponent implements OnInit {
  private authService = inject(AuthService);
  private adminService = inject(AdminService);
  private router = inject(Router);

  public currentUser = signal<any>(null);
  public activeTab = signal<string>('usuarios');
  
  public usuarios = signal<any[]>([]);
  public articulos = signal<any[]>([]);
  public reportes = signal<any[]>([]);
  
  public loadingUsuarios = signal<boolean>(true);
  public loadingArticulos = signal<boolean>(true);
  public loadingReportes = signal<boolean>(true);
  
  public filtroUsuario = signal<string>('');
  public filtroArticulo = signal<string>('');
  public filtroReporte = signal<string>('');

  public showBanModal = signal<boolean>(false);
  public userToBan = signal<any>(null);
  public banDays = signal<number>(7);
  public banReason = signal<string>('');

  ngOnInit() {
    this.authService.currentUser$.subscribe(user => {
      if (!user || user.rol !== 'Administrador') {
        this.router.navigate(['/explorar']);
      } else {
        this.currentUser.set(user);
        this.cargarUsuarios();
        this.cargarArticulos();
        this.cargarReportes();
      }
    });
  }

  cargarUsuarios() {
    this.loadingUsuarios.set(true);
    this.adminService.getUsuarios().subscribe({
      next: (data) => {
        this.usuarios.set(data);
        this.loadingUsuarios.set(false);
      },
      error: (err) => {
        console.error('Error al cargar usuarios:', err);
        this.loadingUsuarios.set(false);
      }
    });
  }

  cargarArticulos() {
    this.loadingArticulos.set(true);
    this.adminService.getArticulosMercado().subscribe({
      next: (data) => {
        this.articulos.set(data);
        this.loadingArticulos.set(false);
      },
      error: (err) => {
        console.error('Error al cargar artículos:', err);
        this.loadingArticulos.set(false);
      }
    });
  }

  cargarReportes() {
    this.loadingReportes.set(true);
    this.adminService.getReportes().subscribe({
      next: (data) => {
        this.reportes.set(data);
        this.loadingReportes.set(false);
      },
      error: (err) => {
        console.error('Error al cargar reportes:', err);
        this.loadingReportes.set(false);
      }
    });
  }

  switchTab(tab: string) {
    this.activeTab.set(tab);
  }

  cambiarEstado(user: any) {
    const nuevoEstado = 1; 
    const userId = user.id || user.idUsuarioReportado;
    const userNombre = user.nombre || user.reportadoNombre;
    const userApellido = user.apellido || '';
    
    if (confirm(`¿Estás seguro de que deseas reactivar (desbanear) al usuario ${userNombre} ${userApellido}?`)) {
      this.adminService.cambiarEstadoUsuario(userId, nuevoEstado).subscribe({
        next: (res) => {
          this.usuarios.set(this.usuarios().map(u => {
            if (u.id === userId) {
              return { ...u, estado: res.estado, motivoBaneo: null, fechaDesbaneo: null };
            }
            return u;
          }));

          this.reportes.set(this.reportes().map(r => {
            if (r.idUsuarioReportado === userId) {
              return { ...r, reportadoEstado: res.estado, reportadoMotivoBaneo: '', reportadoFechaDesbaneo: null };
            }
            return r;
          }));
        },
        error: (err) => console.error('Error al cambiar estado de usuario:', err)
      });
    }
  }

  abrirBanModal(user: any) {
    this.userToBan.set(user);
    this.banDays.set(7);
    this.banReason.set('');
    this.showBanModal.set(true);
  }

  cerrarBanModal() {
    this.showBanModal.set(false);
    this.userToBan.set(null);
  }

  confirmarBaneo() {
    const user = this.userToBan();
    const dias = this.banDays();
    const motivo = this.banReason().trim();
    const userId = user.id || user.idUsuarioReportado;

    if (!userId) return;
    if (dias <= 0) {
      alert('Por favor, ingresa una cantidad de días válida.');
      return;
    }
    if (!motivo) {
      alert('Por favor, ingresa el motivo del baneo.');
      return;
    }

    this.adminService.banearUsuario(userId, dias, motivo).subscribe({
      next: (res) => {
        this.usuarios.set(this.usuarios().map(u => {
          if (u.id === userId) {
            return { ...u, estado: res.estado, motivoBaneo: res.motivoBaneo, fechaDesbaneo: res.fechaDesbaneo };
          }
          return u;
        }));

        this.reportes.set(this.reportes().map(r => {
          if (r.idUsuarioReportado === userId) {
            return { ...r, reportadoEstado: res.estado, reportadoMotivoBaneo: res.motivoBaneo, reportadoFechaDesbaneo: res.fechaDesbaneo };
          }
          return r;
        }));

        this.articulos.set(this.articulos().filter(a => a.idUsuario !== userId));

        this.cerrarBanModal();
        alert(`Usuario baneado exitosamente por ${dias} días.`);
      },
      error: (err) => {
        console.error('Error al banear usuario:', err);
        alert('Hubo un problema al aplicar el baneo.');
      }
    });
  }

  cambiarRol(user: any) {
    const nuevoRol = user.rol === 'Administrador' ? 'Usuario' : 'Administrador';
    const accion = nuevoRol === 'Administrador' ? 'promover a ADMINISTRADOR' : 'degradar a USUARIO COMÚN';

    if (confirm(`¿Estás seguro de que deseas ${accion} al usuario ${user.nombre} ${user.apellido}?`)) {
      this.adminService.cambiarRolUsuario(user.id, nuevoRol).subscribe({
        next: (res) => {
          this.usuarios.set(this.usuarios().map(u => {
            if (u.id === user.id) {
              return { ...u, rol: res.rol };
            }
            return u;
          }));
        },
        error: (err) => {
          console.error('Error al cambiar rol:', err);
          alert('No se pudo actualizar el rol del usuario.');
        }
      });
    }
  }

  eliminarUsuario(user: any) {
    const userId = user.id || user.idUsuarioReportado;
    const userNombre = user.nombre || user.reportadoNombre;
    const userApellido = user.apellido || '';

    if (confirm(`⚠️ ALERTA ⚠️\n¿Estás completamente seguro de eliminar permanentemente al usuario ${userNombre} ${userApellido}?\nEsta acción es irreversible y borrará todo su inventario, mensajes, reportes e historial.`)) {
      this.adminService.eliminarUsuario(userId).subscribe({
        next: () => {
          this.usuarios.set(this.usuarios().filter(u => u.id !== userId));
          this.reportes.set(this.reportes().filter(r => r.idUsuarioReportado !== userId && r.idUsuarioReportante !== userId));
          this.articulos.set(this.articulos().filter(a => a.idUsuario !== userId));
        },
        error: (err) => {
          console.error('Error al eliminar usuario:', err);
          alert('No se pudo eliminar al usuario.');
        }
      });
    }
  }

  eliminarArticulo(art: any) {
    if (confirm(`¿Deseas eliminar la publicación de la carta "${art.nombreCarta}" publicada por "${art.vendedorNombre}"?`)) {
      this.adminService.eliminarArticuloMercado(art.idInventarioUser).subscribe({
        next: () => {
          this.articulos.set(this.articulos().filter(a => a.idInventarioUser !== art.idInventarioUser));
        },
        error: (err) => {
          console.error('Error al eliminar artículo:', art);
          alert('No se pudo eliminar el artículo.');
        }
      });
    }
  }

  desestimarReporte(rep: any) {
    if (confirm(`¿Deseas desestimar el reporte número #${rep.idReporte} enviado por "${rep.reportanteNombre}"?`)) {
      this.adminService.eliminarReporte(rep.idReporte).subscribe({
        next: () => {
          this.reportes.set(this.reportes().filter(r => r.idReporte !== rep.idReporte));
        },
        error: (err) => {
          console.error('Error al desestimar reporte:', err);
          alert('No se pudo desestimar el reporte.');
        }
      });
    }
  }

  getUsuariosFiltrados() {
    const filter = this.filtroUsuario().toLowerCase().trim();
    if (!filter) return this.usuarios();
    
    return this.usuarios().filter(u => 
      u.nombre.toLowerCase().includes(filter) ||
      u.apellido.toLowerCase().includes(filter) ||
      u.correo.toLowerCase().includes(filter) ||
      u.rol.toLowerCase().includes(filter)
    );
  }

  getArticulosFiltrados() {
    const filter = this.filtroArticulo().toLowerCase().trim();
    if (!filter) return this.articulos();

    return this.articulos().filter(a => 
      a.nombreCarta.toLowerCase().includes(filter) ||
      a.vendedorNombre.toLowerCase().includes(filter) ||
      a.vendedorCorreo.toLowerCase().includes(filter) ||
      a.rareza.toLowerCase().includes(filter)
    );
  }

  getReportesFiltrados() {
    const filter = this.filtroReporte().toLowerCase().trim();
    if (!filter) return this.reportes();

    return this.reportes().filter(r => 
      r.reportanteNombre.toLowerCase().includes(filter) ||
      r.reportadoNombre.toLowerCase().includes(filter) ||
      r.motivo.toLowerCase().includes(filter) ||
      r.reportanteCorreo.toLowerCase().includes(filter) ||
      r.reportadoCorreo.toLowerCase().includes(filter)
    );
  }

  formatearFecha(fechaStr: string | null): string {
    if (!fechaStr) return 'N/A';
    const date = new Date(fechaStr);
    return date.toLocaleDateString([], { day: '2-digit', month: 'short', year: 'numeric' });
  }

  formatearFechaHora(fechaStr: string | null): string {
    if (!fechaStr) return 'N/A';
    const date = new Date(fechaStr);
    return date.toLocaleDateString([], { day: '2-digit', month: 'short', hour: '2-digit', minute: '2-digit' });
  }
}
