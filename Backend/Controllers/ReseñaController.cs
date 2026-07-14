using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResenaController : ControllerBase
    {
        private readonly PokemonMarketContext _context;

        public ResenaController(PokemonMarketContext context)
        {
            _context = context;
        }

        // Obtener reseñas de un usuario
        [HttpGet("usuario/{id}")]
        public async Task<IActionResult> GetResenasPorUsuario(int id)
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

            return Ok(resenas);
        }

        // Obtener reseñas de una carta
        [HttpGet("carta/{id}")]
        public async Task<IActionResult> GetResenasPorCarta(int id)
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

            return Ok(resenas);
        }

        // Crear una nueva reseña
        [HttpPost]
        public async Task<IActionResult> PostResena([FromBody] ResenaDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (dto.Calificacion < 1 || dto.Calificacion > 5)
            {
                return BadRequest(new { message = "La calificación debe estar entre 1 y 5." });
            }

            if (dto.IdUsuarioResenado == null && dto.IdItem == null)
            {
                return BadRequest(new { message = "La reseña debe estar asociada a un usuario o a una carta." });
            }

            // Verificar si el usuario ya ha dejado una reseña a este vendedor
            if (dto.IdUsuarioResenado.HasValue)
            {
                var reseñaExistente = await _context.Reseñas
                    .FirstOrDefaultAsync(r => r.IdUsuarioReseñador == dto.IdUsuarioResenador && r.IdUsuarioReseñado == dto.IdUsuarioResenado);
                
                if (reseñaExistente != null)
                {
                    return BadRequest(new { message = "Ya has dejado una reseña para este usuario anteriormente." });
                }

                // NUEVA VALIDACIÓN: Verificar que exista una transacción completada entre ambos
                var transaccionExistente = await _context.Transacciones
                    .AnyAsync(t => t.IdVendedor == dto.IdUsuarioResenado && t.IdComprador == dto.IdUsuarioResenador && t.Estado == "Completado");
                
                if (!transaccionExistente)
                {
                    return BadRequest(new { message = "No puedes reseñar a un vendedor con el que no has completado un trato." });
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

            return Ok(new { message = "Reseña creada con éxito", resena = nuevaResena });
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

    public class ResenaDto
    {
        public int IdUsuarioResenador { get; set; }
        public int? IdUsuarioResenado { get; set; }
        public int? IdItem { get; set; }
        public int Calificacion { get; set; }
        public string Texto { get; set; } = null!;
    }
}
