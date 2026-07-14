using Microsoft.AspNetCore.Mvc;
using Backend.Services.Interfaces;
using Backend.DTOs;
using System;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("usuarios")]
        public async Task<IActionResult> GetUsuarios()
        {
            try
            {
                var usuarios = await _adminService.GetUsuariosAsync();
                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al obtener usuarios: {ex.Message}" });
            }
        }

        [HttpPut("usuarios/estado/{idUsuario}")]
        public async Task<IActionResult> CambiarEstadoUsuario(int idUsuario, [FromBody] int nuevoEstado)
        {
            try
            {
                var (exito, mensaje) = await _adminService.CambiarEstadoUsuarioAsync(idUsuario, nuevoEstado);
                if (!exito)
                {
                    return NotFound(new { mensaje });
                }

                // Get state to return it as it was before, though strictly speaking the frontend just needs success
                return Ok(new { mensaje, estado = nuevoEstado });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al cambiar estado del usuario: {ex.Message}" });
            }
        }

        [HttpPost("usuarios/banear/{idUsuario}")]
        public async Task<IActionResult> BanearUsuario(int idUsuario, [FromBody] BanDto dto)
        {
            try
            {
                var (exito, mensaje) = await _adminService.BanearUsuarioAsync(idUsuario, dto);
                if (!exito)
                {
                    return NotFound(new { mensaje });
                }

                return Ok(new { 
                    mensaje, 
                    estado = 0,
                    motivoBaneo = dto.Motivo,
                    fechaDesbaneo = DateTime.Now.AddDays(dto.Dias)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al banear usuario: {ex.Message}" });
            }
        }

        [HttpDelete("usuarios/eliminar/{idUsuario}")]
        public async Task<IActionResult> EliminarUsuario(int idUsuario)
        {
            try
            {
                var (exito, mensaje) = await _adminService.EliminarUsuarioAsync(idUsuario);
                if (!exito)
                {
                    return NotFound(new { mensaje });
                }

                return Ok(new { mensaje });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al eliminar el usuario: {ex.Message}" });
            }
        }

        [HttpPut("usuarios/rol/{idUsuario}")]
        public async Task<IActionResult> CambiarRolUsuario(int idUsuario, [FromBody] string nuevoRol)
        {
            try
            {
                var (exito, mensaje) = await _adminService.CambiarRolUsuarioAsync(idUsuario, nuevoRol);
                if (!exito)
                {
                    return mensaje.Contains("no encontrado") ? NotFound(new { mensaje }) : BadRequest(new { mensaje });
                }

                return Ok(new { mensaje, rol = nuevoRol });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al cambiar rol del usuario: {ex.Message}" });
            }
        }

        [HttpGet("articulos")]
        public async Task<IActionResult> GetArticulosMercado()
        {
            try
            {
                var articulos = await _adminService.GetArticulosMercadoAsync();
                return Ok(articulos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al obtener artículos del mercado: {ex.Message}" });
            }
        }

        [HttpDelete("articulos/eliminar/{idInventarioUser}")]
        public async Task<IActionResult> EliminarArticuloMercado(int idInventarioUser)
        {
            try
            {
                var (exito, mensaje) = await _adminService.EliminarArticuloMercadoAsync(idInventarioUser);
                if (!exito)
                {
                    return NotFound(new { mensaje });
                }

                return Ok(new { mensaje });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al eliminar el artículo: {ex.Message}" });
            }
        }

        [HttpPost("reportes")]
        public async Task<IActionResult> CrearReporte([FromBody] ReporteDto dto)
        {
            try
            {
                var (exito, mensaje) = await _adminService.CrearReporteAsync(dto);
                return Ok(new { mensaje });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al procesar el reporte: {ex.Message}" });
            }
        }

        [HttpGet("reportes")]
        public async Task<IActionResult> GetReportes()
        {
            try
            {
                var reportes = await _adminService.GetReportesAsync();
                return Ok(reportes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al obtener reportes: {ex.Message}" });
            }
        }

        [HttpDelete("reportes/{idReporte}")]
        public async Task<IActionResult> EliminarReporte(int idReporte)
        {
            try
            {
                var (exito, mensaje) = await _adminService.EliminarReporteAsync(idReporte);
                if (!exito)
                {
                    return NotFound(new { mensaje });
                }

                return Ok(new { mensaje });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al desestimar el reporte: {ex.Message}" });
            }
        }
    }
}
