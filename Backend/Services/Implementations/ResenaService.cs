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
    public class ResenaService : IResenaService
    {
        private readonly PokemonMarketContext _context;

        public ResenaService(PokemonMarketContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<object>> GetResenasPorUsuarioAsync(int id)
        {
            var resenas = await _context.Reseñas
                .Where(r => r.IdUsuarioReseñado == id)
                .Select(r => new {
                    ResenaId = r.ReseñaId,
                    r.Calificacion,
                    r.Texto,
                    r.Fecha,
                    IdUsuarioResenador = r.IdUsuarioReseñador,
                    NombreResenador = r.IdUsuarioReseñadorNavigation != null ? r.IdUsuarioReseñadorNavigation.Nombre : "Anónimo",
                    ImgResenador = r.IdUsuarioReseñadorNavigation != null ? r.IdUsuarioReseñadorNavigation.ImgPerfil : null
                })
                .OrderByDescending(r => r.Fecha)
                .ToListAsync();

            return resenas;
        }

        public async Task<IEnumerable<object>> GetResenasPorCartaAsync(int id)
        {
            var resenas = await _context.Reseñas
                .Where(r => r.IdItem == id)
                .Select(r => new {
                    ResenaId = r.ReseñaId,
                    r.Calificacion,
                    r.Texto,
                    r.Fecha,
                    IdUsuarioResenador = r.IdUsuarioReseñador,
                    NombreResenador = r.IdUsuarioReseñadorNavigation != null ? r.IdUsuarioReseñadorNavigation.Nombre : "Anónimo",
                    ImgResenador = r.IdUsuarioReseñadorNavigation != null ? r.IdUsuarioReseñadorNavigation.ImgPerfil : null
                })
                .OrderByDescending(r => r.Fecha)
                .ToListAsync();

            return resenas;
        }

        public async Task<(bool exito, string mensaje)> PostResenaAsync(ResenaDto dto)
        {
            if (dto.Calificacion < 1 || dto.Calificacion > 5)
            {
                return (false, "La calificación debe estar entre 1 y 5.");
            }

            if (dto.IdUsuarioResenado == null && dto.IdItem == null)
            {
                return (false, "La reseña debe estar asociada a un usuario o a una carta.");
            }

            // Verificar si el usuario ya ha dejado una reseña a este vendedor
            if (dto.IdUsuarioResenado.HasValue)
            {
                var reseñaExistente = await _context.Reseñas
                    .FirstOrDefaultAsync(r => r.IdUsuarioReseñador == dto.IdUsuarioResenador && r.IdUsuarioReseñado == dto.IdUsuarioResenado);
                
                if (reseñaExistente != null)
                {
                    return (false, "Ya has dejado una reseña para este usuario anteriormente.");
                }

                // NUEVA VALIDACIÓN: Verificar que exista una transacción completada entre ambos
                var transaccionExistente = await _context.Transacciones
                    .AnyAsync(t => 
                        (t.IdVendedor == dto.IdUsuarioResenado && t.IdComprador == dto.IdUsuarioResenador && t.Estado == "Completado") ||
                        (t.IdVendedor == dto.IdUsuarioResenador && t.IdComprador == dto.IdUsuarioResenado && t.Estado == "Completado")
                    );
                
                if (!transaccionExistente)
                {
                    return (false, "No puedes reseñar a este usuario porque no registran tratos completados en común.");
                }
            }

            var nuevaResena = new Reseña
            {
                IdUsuarioReseñador = dto.IdUsuarioResenador,
                IdUsuarioReseñado = dto.IdUsuarioResenado,
                IdItem = dto.IdItem,
                Calificacion = dto.Calificacion,
                Texto = dto.Texto,
                Fecha = DateTime.UtcNow
            };

            _context.Reseñas.Add(nuevaResena);
            await _context.SaveChangesAsync();

            // Si la reseña es para un usuario, actualizar su calificación promedio
            if (nuevaResena.IdUsuarioReseñado.HasValue)
            {
                await ActualizarCalificacionUsuario(nuevaResena.IdUsuarioReseñado.Value);
            }

            return (true, "Reseña creada con éxito");
        }

        private async Task ActualizarCalificacionUsuario(int idUsuario)
        {
            var reseñas = await _context.Reseñas
                .Where(r => r.IdUsuarioReseñado == idUsuario)
                .ToListAsync();

            if (reseñas.Any())
            {
                var promedio = reseñas.Average(r => r.Calificacion);
                
                var usuario = await _context.Usuarios.FindAsync(idUsuario);
                if (usuario != null)
                {
                    usuario.Calificacion = (decimal)promedio;
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}
