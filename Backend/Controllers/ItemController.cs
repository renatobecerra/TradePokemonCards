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

        // GET: api/catalogo/guardados/{idUsuario}
        [HttpGet("guardados/{idUsuario}")]
        public async Task<IActionResult> ObtenerGuardados(int idUsuario)
        {
            try
            {
                var guardados = await _context.Guardados
                    .Where(g => g.IdUsuario == idUsuario)
                    .Include(g => g.IdItemNavigation)
                    .Select(g => new {
                        IdLista = g.IdLista,
                        IdItem = g.IdItem,
                        Nombre = g.IdItemNavigation.Nombre,
                        Rareza = g.IdItemNavigation.Rareza,
                        Edicion = g.IdItemNavigation.Edicion,
                        ImgLink = g.IdItemNavigation.ImgLink,
                        IdTgc = g.IdItemNavigation.id_tgc,
                        Precio = g.IdItemNavigation.precio,
                        FechaGuardado = g.FechaGuardado
                    })
                    .ToListAsync();
                return Ok(guardados);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al obtener guardados: {ex.Message}" });
            }
        }

        // DELETE: api/catalogo/guardados/eliminar/{idUsuario}/{idItem}
        [HttpDelete("guardados/eliminar/{idUsuario}/{idItem}")]
        public async Task<IActionResult> EliminarGuardado(int idUsuario, int idItem)
        {
            try
            {
                var guardado = await _context.Guardados
                    .FirstOrDefaultAsync(g => g.IdUsuario == idUsuario && g.IdItem == idItem);

                if (guardado == null)
                {
                    return NotFound(new { mensaje = "Ítem guardado no encontrado." });
                }

                _context.Guardados.Remove(guardado);
                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Ítem removido de tus guardados con éxito." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al eliminar de guardados: {ex.Message}" });
            }
        }

        // POST: api/catalogo/guardar-tgc
        [HttpPost("guardar-tgc")]
        public async Task<IActionResult> GuardarTgcItem([FromBody] GuardarTgcDto datos)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var item = await _context.Inventarios
                    .FirstOrDefaultAsync(i => i.id_tgc == datos.IdTgc);

                if (item == null)
                {
                    item = new Inventario
                    {
                        Nombre = datos.Nombre,
                        Rareza = datos.Rareza,
                        Edicion = datos.Edicion,
                        ImgLink = datos.ImgLink,
                        id_tgc = datos.IdTgc,
                        precio = datos.Precio
                    };
                    _context.Inventarios.Add(item);
                    await _context.SaveChangesAsync();
                }

                var yaGuardado = await _context.Guardados
                    .AnyAsync(g => g.IdUsuario == datos.IdUsuario && g.IdItem == item.IdItem);

                if (yaGuardado)
                {
                    return BadRequest(new { mensaje = "Esta carta ya está en tus deseados." });
                }

                var nuevoGuardado = new Guardado
                {
                    IdUsuario = datos.IdUsuario,
                    IdItem = item.IdItem,
                    FechaGuardado = DateTime.Now
                };

                _context.Guardados.Add(nuevoGuardado);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { mensaje = "Carta agregada a tus deseados con éxito.", idItem = item.IdItem });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { mensaje = $"Error al guardar en deseados: {ex.Message}" });
            }
        }
    }

    public class GuardarItemDto
    {
        public int IdUsuario { get; set; }
        public int IdInventario { get; set; }
    }

    public class GuardarTgcDto
    {
        public int IdUsuario { get; set; }
        public string IdTgc { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string? Rareza { get; set; }
        public string? Edicion { get; set; }
        public string? ImgLink { get; set; }
        public int? Precio { get; set; }
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
        public decimal? Precio { get; set; }
        public int? Cantidad { get; set; }
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

        [HttpGet("vendedores/{idTgc}")]
        public async Task<IActionResult> ObtenerVendedores(string idTgc)
        {
            try
            {
                var vendedores = await _context.InventarioUsuarios
                    .Include(i => i.IdUsuarioNavigation)
                    .Include(i => i.IdItemNavigation)
                    .Where(i => i.IdItemNavigation.id_tgc == idTgc)
                    .Select(i => new {
                        IdInventarioUser = i.IdInventarioUser,
                        IdUsuario = i.IdUsuario,
                        Nombre = i.IdUsuarioNavigation.Nombre,
                        Apellido = i.IdUsuarioNavigation.Apellido,
                        Correo = i.IdUsuarioNavigation.Correo,
                        Telefono = i.IdUsuarioNavigation.Telefono,
                        Foto = i.IdUsuarioNavigation.ImgPerfil,
                        Calificacion = i.IdUsuarioNavigation.Calificacion,
                        EstadoPresencia = i.IdUsuarioNavigation.EstadoPresencia,
                        Estado = i.EstadoFisico,
                        Cantidad = i.Cantidad,
                        Precio = i.IdItemNavigation.precio,
                        NombreCarta = i.IdItemNavigation.Nombre,
                        RarezaCarta = i.IdItemNavigation.Rareza,
                        EdicionCarta = i.IdItemNavigation.Edicion,
                        ImgLinkCarta = i.IdItemNavigation.ImgLink
                    })
                    .ToListAsync();
                return Ok(vendedores);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al obtener vendedores: {ex.Message}" });
            }
        }

        [HttpGet("precio-promedio/{idTgc}")]
        public async Task<IActionResult> ObtenerPrecioPromedio(string idTgc)
        {
            try
            {
                var precios = await _context.InventarioUsuarios
                    .Include(i => i.IdItemNavigation)
                    .Where(i => i.IdItemNavigation.id_tgc == idTgc && i.IdItemNavigation.precio != null && i.IdItemNavigation.precio > 0)
                    .Select(i => i.IdItemNavigation.precio.Value)
                    .ToListAsync();

                if (precios.Count == 0)
                {
                    return Ok(new { promedio = (int?)null });
                }

                double promedio = precios.Average();
                return Ok(new { promedio = (int)Math.Round(promedio) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al calcular promedio: {ex.Message}" });
            }
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

                int cantidadAAgregar = datos.Cantidad ?? 1;

                if (existe != null)
                {
                    // Si ya existe, simplemente incrementamos la cantidad
                    existe.Cantidad = (existe.Cantidad ?? 0) + cantidadAAgregar;
                    // Actualizamos el precio por si ha cambiado
                    existe.IdItemNavigation.precio = (int?)(datos.Precio);
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
                        precio = (int?)(datos.Precio)
                    };
                    
                    _context.Inventarios.Add(nuevoInventario);
                    await _context.SaveChangesAsync();

                    // Creamos el vínculo
                    var vinculo = new InventarioUsuario
                    {
                        IdUsuario = datos.IdUsuario,
                        IdItem = nuevoInventario.IdItem,
                        EstadoFisico = datos.Estado,
                        Cantidad = cantidadAAgregar,
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

                // Actualizamos los datos
                iu.EstadoFisico = datos.Estado;
                iu.Cantidad = datos.Cantidad ?? iu.Cantidad;
                
                // Actualizamos el precio en el ítem base
                iu.IdItemNavigation.precio = (int?)(datos.Precio);

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

                if (iu.Cantidad > 1)
                {
                    iu.Cantidad--;
                    _context.InventarioUsuarios.Update(iu);
                }
                else
                {
                    _context.InventarioUsuarios.Remove(iu);
                }
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
