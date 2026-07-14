using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/mensajes")]
    public class MensajesController : ControllerBase
    {
        private readonly PokemonMarketContext _context;

        public MensajesController(PokemonMarketContext context)
        {
            _context = context;
        }

        public class EnviarMensajeDto
        {
            public int IdRemitente { get; set; }
            public int IdDestinatario { get; set; }
            public string Texto { get; set; } = null!;
            public int? IdItem { get; set; }
        }

        [HttpGet("conversaciones/{usuarioId}")]
        public async Task<IActionResult> GetConversaciones(int usuarioId)
        {
            try
            {
                var userExists = await _context.Usuarios.AnyAsync(u => u.IdUsuarios == usuarioId);
                if (!userExists)
                {
                    return NotFound(new { mensaje = "Usuario no encontrado" });
                }

                var messages = await _context.Mensajes
                    .Where(m => (m.IdRemitente == usuarioId && m.EliminadoPorRemitente != true) || 
                                (m.IdDestinatario == usuarioId && m.EliminadoPorDestinatario != true))
                    .ToListAsync();

                var groupResult = messages
                    .GroupBy(m => m.IdRemitente == usuarioId ? m.IdDestinatario : m.IdRemitente)
                    .Select(g => new
                    {
                        ContactoId = g.Key,
                        UltimoMensaje = g.OrderByDescending(m => m.Fecha ?? DateTime.MinValue).FirstOrDefault()
                    })
                    .ToList();

                var conversaciones = new List<object>();

                foreach (var item in groupResult)
                {
                    var contacto = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuarios == item.ContactoId);
                    if (contacto == null) continue;

                    var unreadCount = messages.Count(m => m.IdRemitente == item.ContactoId && m.IdDestinatario == usuarioId && (m.Estado == false || m.Estado == null));

                    conversaciones.Add(new
                    {
                        contacto = new
                        {
                            id = contacto.IdUsuarios,
                            nombre = contacto.Nombre,
                            apellido = contacto.Apellido,
                            correo = contacto.Correo,
                            foto = contacto.ImgPerfil,
                            estadoPresencia = contacto.EstadoPresencia
                        },
                        ultimoMensaje = new
                        {
                            idMensaje = item.UltimoMensaje?.IdMensaje,
                            texto = item.UltimoMensaje?.Texto,
                            fecha = item.UltimoMensaje?.Fecha,
                            idRemitente = item.UltimoMensaje?.IdRemitente,
                            idDestinatario = item.UltimoMensaje?.IdDestinatario,
                            estado = item.UltimoMensaje?.Estado
                        },
                        noLeidos = unreadCount
                    });
                }

                var orderedConversaciones = conversaciones
                    .OrderByDescending(c => ((dynamic)c).ultimoMensaje.fecha ?? DateTime.MinValue)
                    .ToList();

                return Ok(orderedConversaciones);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al obtener conversaciones: {ex.Message}" });
            }
        }

        [HttpGet("historial/{usuarioId}/{contactoId}")]
        public async Task<IActionResult> GetHistorial(int usuarioId, int contactoId)
        {
            try
            {
                var messages = await _context.Mensajes
                    .Where(m => (m.IdRemitente == usuarioId && m.IdDestinatario == contactoId && m.EliminadoPorRemitente != true) ||
                                (m.IdRemitente == contactoId && m.IdDestinatario == usuarioId && m.EliminadoPorDestinatario != true))
                    .OrderBy(m => m.Fecha)
                    .ToListAsync();

                var unreadMessages = messages.Where(m => m.IdRemitente == contactoId && m.IdDestinatario == usuarioId && (m.Estado == false || m.Estado == null)).ToList();
                if (unreadMessages.Any())
                {
                    foreach (var m in unreadMessages)
                    {
                        m.Estado = true;
                    }
                    await _context.SaveChangesAsync();
                }

                var listado = messages.Select(m => new
                {
                    idMensaje = m.IdMensaje,
                    idRemitente = m.IdRemitente,
                    idDestinatario = m.IdDestinatario,
                    texto = m.Texto,
                    estado = m.Estado,
                    fecha = m.Fecha,
                    idItem = m.IdItem
                }).ToList();

                return Ok(listado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al obtener historial: {ex.Message}" });
            }
        }

        [HttpPost("enviar")]
        public async Task<IActionResult> EnviarMensaje([FromBody] EnviarMensajeDto dto)
        {
            try
            {
                if (dto == null || string.IsNullOrWhiteSpace(dto.Texto))
                {
                    return BadRequest(new { mensaje = "El mensaje no puede estar vacío" });
                }

                var remitenteExiste = await _context.Usuarios.AnyAsync(u => u.IdUsuarios == dto.IdRemitente);
                var destinatarioExiste = await _context.Usuarios.AnyAsync(u => u.IdUsuarios == dto.IdDestinatario);

                if (!remitenteExiste || !destinatarioExiste)
                {
                    return NotFound(new { mensaje = "El remitente o el destinatario no existen" });
                }

                var nuevoMensaje = new Mensaje
                {
                    IdRemitente = dto.IdRemitente,
                    IdDestinatario = dto.IdDestinatario,
                    Texto = dto.Texto,
                    IdItem = dto.IdItem,
                    Estado = false,
                    Fecha = DateTime.Now
                };

                _context.Mensajes.Add(nuevoMensaje);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    idMensaje = nuevoMensaje.IdMensaje,
                    idRemitente = nuevoMensaje.IdRemitente,
                    idDestinatario = nuevoMensaje.IdDestinatario,
                    texto = nuevoMensaje.Texto,
                    estado = nuevoMensaje.Estado,
                    fecha = nuevoMensaje.Fecha,
                    idItem = nuevoMensaje.IdItem
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al enviar mensaje: {ex.Message}" });
            }
        }

        [HttpGet("usuario/{usuarioId}")]
        public async Task<IActionResult> GetUsuarioDetalle(int usuarioId)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .Where(u => u.IdUsuarios == usuarioId)
                    .Select(u => new {
                        id = u.IdUsuarios,
                        nombre = u.Nombre,
                        apellido = u.Apellido,
                        correo = u.Correo,
                        foto = u.ImgPerfil,
                        estadoPresencia = u.EstadoPresencia
                    })
                    .FirstOrDefaultAsync();

                if (usuario == null)
                {
                    return NotFound(new { mensaje = "Usuario no encontrado" });
                }

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al obtener detalle de usuario: {ex.Message}" });
            }
        }

        [HttpDelete("conversacion/{usuarioId}/{contactoId}")]
        public async Task<IActionResult> DeleteConversacion(int usuarioId, int contactoId)
        {
            try
            {
                var messages = await _context.Mensajes
                    .Where(m => (m.IdRemitente == usuarioId && m.IdDestinatario == contactoId) ||
                                (m.IdRemitente == contactoId && m.IdDestinatario == usuarioId))
                    .ToListAsync();

                foreach (var m in messages)
                {
                    if (m.IdRemitente == usuarioId)
                    {
                        m.EliminadoPorRemitente = true;
                    }
                    if (m.IdDestinatario == usuarioId)
                    {
                        m.EliminadoPorDestinatario = true;
                    }
                }

                var toRemove = messages.Where(m => m.EliminadoPorRemitente && m.EliminadoPorDestinatario).ToList();
                if (toRemove.Any())
                {
                    _context.Mensajes.RemoveRange(toRemove);
                }

                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Conversación eliminada con éxito" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al eliminar la conversación: {ex.Message}" });
            }
        }
    }
}
