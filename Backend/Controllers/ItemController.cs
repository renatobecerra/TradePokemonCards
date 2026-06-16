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

    public class CrearItemDto
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; } = null!;
        public string? Estado { get; set; }
        public string? Rareza { get; set; }
        public string? Edicion { get; set; }
        public string? ImgLink { get; set; }
        public string? IdTgc { get; set; }
        public int? Precio { get; set; }
    }

    [ApiController]
    [Route("api/inventario")]
    public class InventarioController : ControllerBase
    {
        private readonly PokemonMarketContext _context;

        public InventarioController(PokemonMarketContext context)
        {
            _context = context;
        }

        [HttpGet("{idUsuario}")]
        public async Task<IActionResult> ObtenerInventario(int idUsuario)
        {
            try
            {
                var items = await _context.InventarioUsuarios
                    .Where(i => i.IdUsuario == idUsuario)
                    .Include(i => i.IdItemNavigation)
                    .Select(i => new {
                        IdInventarioUser = i.IdInventarioUser,
                        IdItem = i.IdItem,
                        Nombre = i.IdItemNavigation.Nombre,
                        Estado = i.EstadoFisico,
                        Rareza = i.IdItemNavigation.Rareza,
                        Edicion = i.IdItemNavigation.Edicion,
                        ImgLink = i.IdItemNavigation.ImgLink,
                        IdTgc = i.IdItemNavigation.id_tgc,
                        Precio = i.IdItemNavigation.precio,
                        Cantidad = i.Cantidad
                    })
                    .ToListAsync();
                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al obtener el inventario: {ex.Message}" });
            }
        }

        [HttpPost("agregar")]
        public async Task<IActionResult> AgregarAlInventario([FromBody] CrearItemDto datos)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Buscamos si ya existe una carta idéntica en el inventario del usuario
                // Criterio de "igual": mismo ID de TCGdex (o nombre/edición si no hay ID) y MISMO ESTADO FÍSICO
                var existe = await _context.InventarioUsuarios
                    .Include(iu => iu.IdItemNavigation)
                    .FirstOrDefaultAsync(iu => 
                        iu.IdUsuario == datos.IdUsuario && 
                        iu.EstadoFisico == datos.Estado &&
                        (datos.IdTgc != null ? iu.IdItemNavigation.id_tgc == datos.IdTgc : iu.IdItemNavigation.Nombre == datos.Nombre && iu.IdItemNavigation.Edicion == datos.Edicion)
                    );

                if (existe != null)
                {
                    // Si ya existe, simplemente incrementamos la cantidad
                    existe.Cantidad = (existe.Cantidad ?? 0) + 1;
                    _context.InventarioUsuarios.Update(existe);
                }
                else
                {
                    // Si no existe, creamos el ítem base
                    var nuevoInventario = new Inventario
                    {
                        Nombre = datos.Nombre,
                        Rareza = datos.Rareza,
                        Edicion = datos.Edicion,
                        ImgLink = datos.ImgLink,
                        id_tgc = datos.IdTgc,
                        precio = datos.Precio
                    };
                    
                    _context.Inventarios.Add(nuevoInventario);
                    await _context.SaveChangesAsync();

                    // Creamos el vínculo
                    var vinculo = new InventarioUsuario
                    {
                        IdUsuario = datos.IdUsuario,
                        IdItem = nuevoInventario.IdItem,
                        EstadoFisico = datos.Estado,
                        Cantidad = 1,
                        FechaObtencion = DateTime.Now
                    };

                    _context.InventarioUsuarios.Add(vinculo);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(new { mensaje = "Operación realizada con éxito." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { mensaje = $"Error al procesar inventario: {ex.Message}" });
            }
        }

        [HttpPut("editar/{idInventarioUser}")]
        public async Task<IActionResult> EditarItem(int idInventarioUser, [FromBody] CrearItemDto datos)
        {
            try
            {
                var iu = await _context.InventarioUsuarios
                    .Include(i => i.IdItemNavigation)
                    .FirstOrDefaultAsync(i => i.IdInventarioUser == idInventarioUser);

                if (iu == null) return NotFound(new { mensaje = "Ítem no encontrado." });

                // Actualizamos el estado físico en el vínculo
                iu.EstadoFisico = datos.Estado;
                
                // Actualizamos el precio en el ítem base (asociado a este registro)
                iu.IdItemNavigation.precio = datos.Precio;

                await _context.SaveChangesAsync();
                return Ok(new { mensaje = "Carta actualizada con éxito." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al editar: {ex.Message}" });
            }
        }

        [HttpDelete("eliminar/{idInventarioUser}")]
        public async Task<IActionResult> EliminarItem(int idInventarioUser)
        {
            try
            {
                var iu = await _context.InventarioUsuarios.FindAsync(idInventarioUser);
                if (iu == null) return NotFound(new { mensaje = "Ítem no encontrado." });

                _context.InventarioUsuarios.Remove(iu);
                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Carta eliminada de tu inventario." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al eliminar: {ex.Message}" });
            }
        }
    }
}
