using Microsoft.AspNetCore.Mvc;
using Backend.Services.Interfaces;
using Backend.Models;
using Backend.DTOs;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/catalogo")]
    public class ItemController : ControllerBase
    {
        private readonly IItemService _itemService;

        public ItemController(IItemService itemService)
        {
            _itemService = itemService;
        }

        
        [HttpGet]
        public async Task<IActionResult> ObtenerCatalogo()
        {
            try
            {
                var items = await _itemService.ObtenerCatalogoAsync();
                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al obtener el catálogo: {ex.Message}" });
            }
        }

       
        [HttpPost("guardar")]
        public async Task<IActionResult> GuardarItem([FromBody] GuardarItemDto datos)
        {
            try
            {
                var (exito, mensaje) = await _itemService.GuardarItemAsync(datos);
                if (!exito)
                {
                    return BadRequest(new { mensaje });
                }

                return Ok(new { mensaje });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al guardar: {ex.Message}" });
            }
        }

        
        [HttpGet("guardados/{idUsuario}")]
        public async Task<IActionResult> ObtenerGuardados(int idUsuario)
        {
            try
            {
                var guardados = await _itemService.ObtenerGuardadosAsync(idUsuario);
                return Ok(guardados);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al obtener guardados: {ex.Message}" });
            }
        }

        
        [HttpDelete("guardados/eliminar/{idUsuario}/{idItem}")]
        public async Task<IActionResult> EliminarGuardado(int idUsuario, int idItem)
        {
            try
            {
                var (exito, mensaje) = await _itemService.EliminarGuardadoAsync(idUsuario, idItem);
                if (!exito)
                {
                    return NotFound(new { mensaje });
                }

                return Ok(new { mensaje });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al eliminar de guardados: {ex.Message}" });
            }
        }

        
        [HttpPost("guardar-tgc")]
        public async Task<IActionResult> GuardarTgcItem([FromBody] GuardarTgcDto datos)
        {
            try
            {
                var (exito, mensaje, idItem) = await _itemService.GuardarTgcItemAsync(datos);
                if (!exito)
                {
                    return BadRequest(new { mensaje });
                }

                return Ok(new { mensaje, idItem });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al guardar en deseados: {ex.Message}" });
            }
        }

        
        [HttpGet("top")]
        public async Task<IActionResult> ObtenerTopRegistros()
        {
            try
            {
                var items = await _itemService.ObtenerTopRegistrosAsync();
                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al obtener el top de registros: {ex.Message}" });
            }
        }
    }
}
