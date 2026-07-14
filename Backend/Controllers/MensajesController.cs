using Microsoft.AspNetCore.Mvc;
using Backend.Services.Interfaces;
using Backend.DTOs;
using System;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/mensajes")]
    public class MensajesController : ControllerBase
    {
        private readonly IMensajesService _mensajesService;

        public MensajesController(IMensajesService mensajesService)
        {
            _mensajesService = mensajesService;
        }

        [HttpGet("conversaciones/{usuarioId}")]
        public async Task<IActionResult> GetConversaciones(int usuarioId)
        {
            try
            {
                var conversaciones = await _mensajesService.GetConversacionesAsync(usuarioId);
                return Ok(conversaciones);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { mensaje = ex.Message });
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
                var historial = await _mensajesService.GetHistorialAsync(usuarioId, contactoId);
                return Ok(historial);
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
                var (exito, mensaje, mensajeObj) = await _mensajesService.EnviarMensajeAsync(dto);
                if (!exito)
                {
                    return mensaje.Contains("no existen") ? NotFound(new { mensaje }) : BadRequest(new { mensaje });
                }

                return Ok(mensajeObj);
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
                var (exito, mensaje, usuario) = await _mensajesService.GetUsuarioDetalleAsync(usuarioId);
                if (!exito)
                {
                    return NotFound(new { mensaje });
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
                var (exito, mensaje) = await _mensajesService.DeleteConversacionAsync(usuarioId, contactoId);
                if (!exito)
                {
                    return NotFound(new { mensaje });
                }

                return Ok(new { mensaje });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error al eliminar la conversación: {ex.Message}" });
            }
        }
    }
}
