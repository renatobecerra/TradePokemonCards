using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/catalogo")]
    public class ItemController : ControllerBase
    {
        private readonly PokemonMarketContext _context;

        public ItemController(PokemonMarketContext context)
        {
            _context = context;
        }

        // GET: api/catalogo
        [HttpGet]
        public async Task<IActionResult> ObtenerCatalogo()
        {
            try
            {
                var items = await _context.Inventarios.ToListAsync();
                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al obtener el catálogo: {ex.Message}" });
            }
        }

        // POST: api/catalogo/guardar
        [HttpPost("guardar")]
        public async Task<IActionResult> GuardarItem([FromBody] GuardarItemDto datos)
        {
            try
            {
                var yaGuardado = await _context.Guardados
                    .AnyAsync(g => g.IdUsuario == datos.IdUsuario && g.IdItem == datos.IdInventario);

                if (yaGuardado)
                {
                    return BadRequest(new { mensaje = "Este ítem ya está en tu lista de guardados." });
                }

                var nuevoGuardado = new Guardado 
                {
                    IdUsuario = datos.IdUsuario,
                    IdItem = datos.IdInventario,
                    FechaGuardado = DateTime.Now
                };

                _context.Guardados.Add(nuevoGuardado);
                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Ítem guardado en tu lista con éxito." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al guardar: {ex.Message}" });
            }
        }
    }

    public class GuardarItemDto
    {
        public int IdUsuario { get; set; }
        public int IdInventario { get; set; }
    }
}
