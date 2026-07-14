using Backend.Models;
using Backend.DTOs;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Services.Implementations
{
    public class MensajesService : IMensajesService
    {
        private readonly PokemonMarketContext _context;

        public MensajesService(PokemonMarketContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<object>> GetConversacionesAsync(int usuarioId)
        {
            var userExists = await _context.Usuarios.AnyAsync(u => u.IdUsuarios == usuarioId);
            if (!userExists)
            {
                throw new ArgumentException("Usuario no encontrado");
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

            return orderedConversaciones;
        }

        public async Task<IEnumerable<object>> GetHistorialAsync(int usuarioId, int contactoId)
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

            return listado;
        }

        public async Task<(bool exito, string mensaje, object? mensajeObj)> EnviarMensajeAsync(EnviarMensajeDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Texto))
            {
                return (false, "El mensaje no puede estar vacío", null);
            }

            var remitenteExiste = await _context.Usuarios.AnyAsync(u => u.IdUsuarios == dto.IdRemitente);
            var destinatarioExiste = await _context.Usuarios.AnyAsync(u => u.IdUsuarios == dto.IdDestinatario);

            if (!remitenteExiste || !destinatarioExiste)
            {
                return (false, "El remitente o el destinatario no existen", null);
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

            var mensajeRetorno = new
            {
                idMensaje = nuevoMensaje.IdMensaje,
                idRemitente = nuevoMensaje.IdRemitente,
                idDestinatario = nuevoMensaje.IdDestinatario,
                texto = nuevoMensaje.Texto,
                estado = nuevoMensaje.Estado,
                fecha = nuevoMensaje.Fecha,
                idItem = nuevoMensaje.IdItem
            };

            return (true, "Mensaje enviado exitosamente", mensajeRetorno);
        }

        public async Task<(bool exito, string mensaje, object? usuario)> GetUsuarioDetalleAsync(int usuarioId)
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
                return (false, "Usuario no encontrado", null);
            }

            return (true, "", usuario);
        }

        public async Task<(bool exito, string mensaje)> DeleteConversacionAsync(int usuarioId, int contactoId)
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

            var toRemove = messages.Where(m => m.EliminadoPorRemitente == true && m.EliminadoPorDestinatario == true).ToList();
            if (toRemove.Any())
            {
                _context.Mensajes.RemoveRange(toRemove);
            }

            await _context.SaveChangesAsync();

            return (true, "Conversación eliminada con éxito");
        }
    }
}
