using Microsoft.AspNetCore.Mvc;
using Backend.Services.Interfaces;
using Backend.Models;
using Backend.DTOs;
using System;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("google")]
        public async Task<IActionResult> GoogleLogin([FromBody] string credential)
        {
            try
            {
                var (exito, mensaje, usuario) = await _authService.GoogleLoginAsync(credential);
                if (!exito)
                {
                    return BadRequest(new { mensaje });
                }

                return Ok(new { mensaje, usuario });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Token de Google inválido: " + ex.Message });
            }
        }

        [HttpPost("cambiar-presencia")]
        public async Task<IActionResult> CambiarPresencia([FromBody] CambiarPresenciaDto datos)
        {
            try
            {
                var (exito, mensaje) = await _authService.CambiarPresenciaAsync(datos);
                if (!exito)
                {
                    return mensaje.Contains("no encontrado") ? NotFound(new { mensaje }) : BadRequest(new { mensaje });
                }

                return Ok(new { mensaje, nuevoEstado = datos.NuevoEstado });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al actualizar presencia: " + ex.Message });
            }
        }

        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar([FromBody] Usuario nuevoUsuario)
        {
            try
            {
                var (exito, mensaje, usuario) = await _authService.RegistrarAsync(nuevoUsuario);
                if (!exito)
                {
                    return BadRequest(new { mensaje });
                }

                return Ok(new { mensaje });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error interno del servidor: {ex.Message}" });
            }
        }

        [HttpPost("verificar")]
        public async Task<IActionResult> Verificar([FromBody] VerificarDto datosVerificacion)
        {
            try
            {
                var (exito, mensaje, _) = await _authService.VerificarAsync(datosVerificacion);
                if (!exito)
                {
                    return mensaje.Contains("no encontrado") ? NotFound(new { mensaje }) : BadRequest(new { mensaje });
                }

                return Ok(new { mensaje });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error Interno: {ex.Message}" });
            }
        }

        [HttpPost("solicitar-recuperacion")]
        public async Task<IActionResult> SolicitarRecuperacion([FromBody] RecoveryRequestDto request)
        {
            try
            {
                var (exito, mensaje) = await _authService.SolicitarRecuperacionAsync(request);
                if (!exito)
                {
                    return mensaje.Contains("registrado") ? NotFound(new { mensaje }) : BadRequest(new { mensaje });
                }

                return Ok(new { mensaje });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error: " + ex.Message });
            }
        }

        [HttpPost("validar-codigo-recuperacion")]
        public async Task<IActionResult> ValidarCodigoRecuperacion([FromBody] VerificarDto datos)
        {
            try
            {
                var (exito, mensaje) = await _authService.ValidarCodigoRecuperacionAsync(datos);
                if (!exito)
                {
                    return BadRequest(new { mensaje });
                }

                return Ok(new { mensaje });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en ValidarCodigoRecuperacion: {ex.Message}");
                return StatusCode(500, new { mensaje = "Error interno al validar el código." });
            }
        }

        [HttpPost("resetear-password")]
        public async Task<IActionResult> ResetearPassword([FromBody] ResetPasswordDto datos)
        {
            try
            {
                var (exito, mensaje) = await _authService.ResetearPasswordAsync(datos);
                if (!exito)
                {
                    return BadRequest(new { mensaje });
                }

                return Ok(new { mensaje });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en ResetearPassword: {ex.Message}");
                return StatusCode(500, new { mensaje = "Error interno al actualizar la contraseña." });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto datosLogin)
        {
            try
            {
                var (exito, mensaje, usuario) = await _authService.LoginAsync(datosLogin);
                if (!exito)
                {
                    if (mensaje == "requiere_verificacion")
                    {
                        return Unauthorized(new { mensaje = "Por favor, verifica tu cuenta a través de tu correo electrónico antes de iniciar sesión.", requiereVerificacion = true });
                    }
                    return Unauthorized(new { mensaje });
                }

                return Ok(new { mensaje, usuario });
            }
            catch(Exception ex)
            {
                return StatusCode(500, new {mensaje = $"Error Interno: {ex.Message}"});
            }
        }

        [HttpGet("hacer-admin")]
        public async Task<IActionResult> HacerAdmin([FromQuery] string correo)
        {
            try
            {
                var (exito, mensaje) = await _authService.HacerAdminAsync(correo);
                if (!exito)
                {
                    return mensaje.Contains("no encontrado") ? NotFound(new { mensaje }) : BadRequest(new { mensaje });
                }

                return Ok(new { mensaje });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error: {ex.Message}" });
            }
        }

        [HttpGet("publico/{id}")]
        public async Task<IActionResult> GetPerfilPublico(int id)
        {
            try
            {
                var (exito, mensaje, perfil) = await _authService.GetPerfilPublicoAsync(id);
                if (!exito)
                {
                    return NotFound(new { mensaje });
                }

                return Ok(perfil);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener perfil público: " + ex.Message });
            }
        }

        [HttpPost("actualizar-perfil")]
        public async Task<IActionResult> ActualizarPerfil([FromBody] ActualizarPerfilDto datos)
        {
            try
            {
                var (exito, mensaje) = await _authService.ActualizarPerfilAsync(datos);
                if (!exito)
                {
                    return NotFound(new { mensaje });
                }

                // Obtener perfil actualizado para retornar
                var (_, _, perfil) = await _authService.GetPerfilPublicoAsync(datos.UsuarioId);
                
                return Ok(new
                {
                    mensaje,
                    usuario = perfil
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al actualizar perfil: " + ex.Message });
            }
        }

        [HttpPost("cambiar-password")]
        public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordDto datos)
        {
            try
            {
                var (exito, mensaje) = await _authService.CambiarPasswordAsync(datos);
                if (!exito)
                {
                    return mensaje.Contains("no encontrado") ? NotFound(new { mensaje }) : BadRequest(new { mensaje });
                }

                return Ok(new { mensaje });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al cambiar contraseña: " + ex.Message });
            }
        }
    }
}
