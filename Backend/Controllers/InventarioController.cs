using Microsoft.AspNetCore.Mvc;
using Backend.Services.Interfaces;
using Backend.Models;
using Backend.DTOs;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/inventario")]
    public class InventarioController : ControllerBase
    {
        private readonly IInventarioService _inventarioService;

        public InventarioController(IInventarioService inventarioService)
        {
            _inventarioService = inventarioService;
        }

        [HttpGet("vendedores/{idTgc}")]
        public async Task<IActionResult> ObtenerVendedores(string idTgc)
        {
            try
            {
                var vendedores = await _inventarioService.ObtenerVendedoresAsync(idTgc);
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
                var promedio = await _inventarioService.ObtenerPrecioPromedioAsync(idTgc);
                return Ok(new { promedio });
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
                var items = await _inventarioService.ObtenerInventarioAsync(idUsuario);
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
            try
            {
                var (exito, mensaje) = await _inventarioService.AgregarAlInventarioAsync(datos);
                return Ok(new { mensaje });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al procesar inventario: {ex.Message}" });
            }
        }

        [HttpPut("editar/{idInventarioUser}")]
        public async Task<IActionResult> EditarItem(int idInventarioUser, [FromBody] CrearItemDto datos)
        {
            try
            {
                var (exito, mensaje) = await _inventarioService.EditarItemAsync(idInventarioUser, datos);
                if (!exito) return NotFound(new { mensaje });

                return Ok(new { mensaje });
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
                var (exito, mensaje) = await _inventarioService.EliminarItemAsync(idInventarioUser);
                if (!exito) return NotFound(new { mensaje });

                return Ok(new { mensaje });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al eliminar: {ex.Message}" });
            }
        }
    }
}
