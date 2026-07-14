import { Component, OnInit, OnDestroy, signal, inject, ViewChild, ElementRef, AfterViewChecked, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../services/auth.service';
import { MensajesService } from '../services/mensajes.service';
import { AdminService } from '../services/admin.service';
import { CompNavBarComponent } from '../comp-nav-bar/comp-nav-bar';

@Component({
  selector: 'app-mensajes',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, CompNavBarComponent],
  templateUrl: './mensajes.html',
  styleUrls: ['./mensajes.css']
})
export class MensajesComponent implements OnInit, OnDestroy, AfterViewChecked {
  private authService = inject(AuthService);
  private mensajesService = inject(MensajesService);
  private adminService = inject(AdminService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  @ViewChild('scrollContainer') private scrollContainer!: ElementRef;
  @ViewChild('fileInput') private fileInput!: ElementRef;

  public currentUser = signal<any>(null);
  public conversaciones = signal<any[]>([]);
  public seleccionado = signal<any>(null);
  public mensajes = signal<any[]>([]);
  public textoMensaje = signal<string>('');
  
  public loadingChats = signal<boolean>(true);
  public loadingMensajes = signal<boolean>(false);

  public showOptionsMenu = signal<boolean>(false);

  private pollSubscription: any = null;
  private shouldScrollToBottom = false;

  ngOnInit() {
    this.authService.currentUser$.subscribe(user => {
      if (!user) {
        this.router.navigate(['/login']);
      } else {
        this.currentUser.set(user);
        this.cargarConversacionesYProcesarQuery();
      }
    });

    this.pollSubscription = setInterval(() => {
      this.actualizarChatsYMensajes();
    }, 4000);
  }

  ngOnDestroy() {
    if (this.pollSubscription) {
      clearInterval(this.pollSubscription);
    }
  }

  ngAfterViewChecked() {
    if (this.shouldScrollToBottom) {
      this.scrollToBottom();
      this.shouldScrollToBottom = false;
    }
  }

  cargarConversacionesYProcesarQuery() {
    const user = this.currentUser();
    if (!user) return;

    this.loadingChats.set(true);
    this.mensajesService.getConversaciones(user.id).subscribe({
      next: (convs) => {
        this.conversaciones.set(convs);
        this.loadingChats.set(false);

        this.route.queryParams.subscribe(params => {
          const contactIdParam = params['contactId'] || params['usuarioId'] || params['sellerId'];
          if (contactIdParam) {
            const cId = parseInt(contactIdParam, 10);
            
            const convExistente = convs.find(c => c.contacto.id === cId);
            if (convExistente) {
              this.seleccionarConversacion(convExistente);
            } else {
              this.mensajesService.getUsuarioDetalle(cId).subscribe({
                next: (contactoInfo) => {
                  const nuevaConvTemp = {
                    contacto: contactoInfo,
                    ultimoMensaje: null,
                    noLeidos: 0,
                    esTemporal: true
                  };
                  this.conversaciones.set([nuevaConvTemp, ...convs]);
                  this.seleccionarConversacion(nuevaConvTemp);
                },
                error: (err) => console.error('Error al cargar detalle del usuario de la consulta:', err)
              });
            }
          } else if (convs.length > 0 && !this.seleccionado()) {
            this.seleccionarConversacion(convs[0]);
          }
        });
      },
      error: (err) => {
        console.error('Error cargando conversaciones:', err);
        this.loadingChats.set(false);
      }
    });
  }

  seleccionarConversacion(conv: any) {
    this.showOptionsMenu.set(false);
    this.seleccionado.set(conv);
    this.mensajes.set([]);
    this.loadingMensajes.set(true);
    this.shouldScrollToBottom = true;

    const user = this.currentUser();
    if (!user) return;

    this.mensajesService.getHistorial(user.id, conv.contacto.id).subscribe({
      next: (msgs) => {
        this.mensajes.set(msgs);
        this.loadingMensajes.set(false);
        this.shouldScrollToBottom = true;
        
        conv.noLeidos = 0;
        
        this.mensajesService.actualizarContadorPendientes(user.id);
      },
      error: (err) => {
        console.error('Error al cargar historial:', err);
        this.loadingMensajes.set(false);
      }
    });
  }

  actualizarChatsYMensajes() {
    const user = this.currentUser();
    if (!user) return;

    this.mensajesService.getConversaciones(user.id).subscribe({
      next: (convs) => {
        const seleccionadoActual = this.seleccionado();
        let convsActualizadas = [...convs];

        if (seleccionadoActual && seleccionadoActual.esTemporal) {
          const yaExisteEnBackend = convs.some(c => c.contacto.id === seleccionadoActual.contacto.id);
          if (!yaExisteEnBackend) {
            convsActualizadas = [seleccionadoActual, ...convs];
          } else {
            seleccionadoActual.esTemporal = false;
          }
        }

        this.conversaciones.set(convsActualizadas);

        if (seleccionadoActual) {
          this.mensajesService.getHistorial(user.id, seleccionadoActual.contacto.id).subscribe({
            next: (msgs) => {
              if (msgs.length > this.mensajes().length) {
                this.shouldScrollToBottom = true;
              }
              this.mensajes.set(msgs);
            }
          });
        }
      }
    });
  }

  enviar() {
    const texto = this.textoMensaje().trim();
    const user = this.currentUser();
    const sel = this.seleccionado();
    
    if (!texto || !user || !sel) return;

    this.textoMensaje.set('');

    this.mensajesService.enviarMensaje(user.id, sel.contacto.id, texto).subscribe({
      next: (nuevoMsg) => {
        this.mensajes.set([...this.mensajes(), nuevoMsg]);
        this.shouldScrollToBottom = true;

        sel.ultimoMensaje = nuevoMsg;
        if (sel.esTemporal) {
          sel.esTemporal = false;
        }

        const listaConvs = this.conversaciones().filter(c => c.contacto.id !== sel.contacto.id);
        this.conversaciones.set([sel, ...listaConvs]);
        
        this.mensajesService.actualizarContadorPendientes(user.id);
      },
      error: (err) => console.error('Error enviando mensaje:', err)
    });
  }

  triggerFileInput() {
    this.fileInput.nativeElement.click();
  }

  onFileSelected(event: any) {
    const file = event.target.files?.[0];
    if (!file) return;

    if (!file.type.startsWith('image/')) {
      alert('Por favor selecciona una archivo de imagen válido (.png, .jpg, .jpeg, .gif, etc.)');
      return;
    }

    const reader = new FileReader();
    reader.onload = (e: any) => {
      const rawBase64 = e.target.result;
      this.compressImage(rawBase64, (compressedBase64) => {
        this.enviarMensajeImagen(compressedBase64);
      });
    };
    reader.readAsDataURL(file);

    event.target.value = '';
  }

  compressImage(dataUrl: string, callback: (result: string) => void) {
    const img = new Image();
    img.onload = () => {
      const canvas = document.createElement('canvas');
      let width = img.width;
      let height = img.height;
      
      const maxDimension = 500;
      if (width > maxDimension || height > maxDimension) {
        if (width > height) {
          height = Math.round((height * maxDimension) / width);
          width = maxDimension;
        } else {
          width = Math.round((width * maxDimension) / height);
          height = maxDimension;
        }
      }

      canvas.width = width;
      canvas.height = height;
      const ctx = canvas.getContext('2d');
      if (ctx) {
        ctx.drawImage(img, 0, 0, width, height);
        const compressed = canvas.toDataURL('image/jpeg', 0.65);
        callback(compressed);
      } else {
        callback(dataUrl);
      }
    };
    img.src = dataUrl;
  }

  enviarMensajeImagen(base64Data: string) {
    const user = this.currentUser();
    const sel = this.seleccionado();
    if (!user || !sel || !base64Data) return;

    this.mensajesService.enviarMensaje(user.id, sel.contacto.id, base64Data).subscribe({
      next: (nuevoMsg) => {
        this.mensajes.set([...this.mensajes(), nuevoMsg]);
        this.shouldScrollToBottom = true;

        sel.ultimoMensaje = nuevoMsg;
        if (sel.esTemporal) {
          sel.esTemporal = false;
        }

        const listaConvs = this.conversaciones().filter(c => c.contacto.id !== sel.contacto.id);
        this.conversaciones.set([sel, ...listaConvs]);
        
        this.mensajesService.actualizarContadorPendientes(user.id);
      },
      error: (err) => console.error('Error al enviar mensaje con imagen:', err)
    });
  }

  esImagen(texto: string | null | undefined): boolean {
    return !!texto && texto.startsWith('data:image/');
  }

  eliminarConversacionActual() {
    const user = this.currentUser();
    const sel = this.seleccionado();
    if (!user || !sel) return;

    if (confirm(`¿Estás seguro de que deseas eliminar la conversación con ${sel.contacto.nombre} ${sel.contacto.apellido}? Se borrará permanentemente todo el historial.`)) {
      this.mensajesService.deleteConversacion(user.id, sel.contacto.id).subscribe({
        next: () => {
          const nuevasConvs = this.conversaciones().filter(c => c.contacto.id !== sel.contacto.id);
          this.conversaciones.set(nuevasConvs);
          
          this.showOptionsMenu.set(false);
          this.seleccionado.set(null);
          this.mensajes.set([]);
          
          this.mensajesService.actualizarContadorPendientes(user.id);
        },
        error: (err) => {
          console.error('Error al eliminar conversación:', err);
          alert('Hubo un problema al eliminar la conversación.');
        }
      });
    }
  }

  toggleOptionsMenu(event: Event) {
    event.stopPropagation();
    this.showOptionsMenu.set(!this.showOptionsMenu());
  }

  scrollToBottom(): void {
    try {
      this.scrollContainer.nativeElement.scrollTop = this.scrollContainer.nativeElement.scrollHeight;
    } catch (err) {
    }
  }

  getPresenceColor(estado: number): string {
    switch (estado) {
      case 1: return '#2ecc71';
      case 2: return '#e74c3c';
      case 0: return '#95afc0';
      default: return '#2ecc71';
    }
  }

  getPresenceLabel(estado: number): string {
    switch (estado) {
      case 1: return 'Conectado';
      case 2: return 'No molestar';
      case 0: return 'Desconectado';
      default: return 'Conectado';
    }
  }

  formatearFecha(fechaStr: string | null): string {
    if (!fechaStr) return '';
    const fecha = new Date(fechaStr);
    
    const hoy = new Date();
    if (fecha.toDateString() === hoy.toDateString()) {
      return fecha.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    }
    return fecha.toLocaleDateString([], { day: '2-digit', month: 'short' });
  }

  formatearHoraCompleta(fechaStr: string | null): string {
    if (!fechaStr) return '';
    const fecha = new Date(fechaStr);
    return fecha.toLocaleDateString([], { day: 'numeric', month: 'short', hour: '2-digit', minute: '2-digit' });
  }

  reportarEntrenadorActual() {
    const user = this.currentUser();
    const sel = this.seleccionado();
    if (!user || !sel) return;

    this.showOptionsMenu.set(false);

    const motivo = prompt(`Reportar a ${sel.contacto.nombre} ${sel.contacto.apellido}\nEscribe el motivo del reporte (comportamiento ofensivo, intento de estafa, etc.):`);
    if (motivo === null) return;
    
    const motivoTrimmed = motivo.trim();
    if (!motivoTrimmed) {
      alert('Debes ingresar un motivo válido para realizar el reporte.');
      return;
    }

    const payload = {
      IdUsuarioReportante: user.id,
      IdUsuarioReportado: sel.contacto.id,
      Motivo: motivoTrimmed
    };

    this.adminService.crearReporte(payload).subscribe({
      next: () => {
        alert('Reporte enviado con éxito. Un administrador lo revisará a la brevedad.');
      },
      error: (err) => {
        console.error('Error al enviar reporte:', err);
        alert('Hubo un problema al enviar el reporte. Por favor inténtalo de nuevo.');
      }
    });
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: Event) {
    if (this.showOptionsMenu()) {
      const container = document.querySelector('.options-menu-container');
      if (container && !container.contains(event.target as Node)) {
        this.showOptionsMenu.set(false);
      }
    }
  }
}
