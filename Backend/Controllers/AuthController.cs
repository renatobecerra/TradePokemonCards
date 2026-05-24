using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using BCrypt.Net;
using Backend.Services;
using Google.Apis.Auth;
using System.Text.RegularExpressions;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private const string PasswordPolicyMessage = "La contraseña debe tener al menos 8 caracteres, una mayúscula, un número y un símbolo.";
        private static readonly Regex PasswordPolicy = new(@"^(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]).{8,}$", RegexOptions.Compiled);

        private readonly PokemonMarketContext _context;
        private readonly IEmailService _emailService;

        public AuthController(PokemonMarketContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpPost("google")]
        public async Task<IActionResult> GoogleLogin([FromBody] string credential)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new List<string>() { "929961623910-oas1o9fve1r93c1mppd2j7f694p0oht6.apps.googleusercontent.com" }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(credential, settings);

                var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == payload.Email);

                if (usuario == null)
                {
                    usuario = new Usuario
                    {
                        Nombre = payload.GivenName,
                        Apellido = payload.FamilyName,
                        Correo = payload.Email,
                        Contraseña = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
                        EsVerificado = true,
                        Rol = "Usuario",
                        FechaRegistro = DateTime.Now,
                        ImgPerfil = payload.Picture,
                        EstadoPresencia = 1
                    };
                    _context.Usuarios.Add(usuario);
                    await _context.SaveChangesAsync();
                }

                return Ok(new
                {
                    mensaje = "Inicio de sesión con Google exitoso.",
                    usuario = new
                    {
                        id = usuario.IdUsuarios,
                        nombre = usuario.Nombre,
                        apellido = usuario.Apellido,
                        rol = usuario.Rol,
                        foto = usuario.ImgPerfil,
                        estadoPresencia = usuario.EstadoPresencia,
                        bio = usuario.Descripcion
                    }
                });
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
                var usuario = await _context.Usuarios.FindAsync(datos.UsuarioId);
                if (usuario == null) return NotFound(new { mensaje = "Usuario no encontrado" });

                if (datos.NuevoEstado < 0 || datos.NuevoEstado > 2)
                {
                    return BadRequest(new { mensaje = "Estado no válido" });
                }

                usuario.EstadoPresencia = (sbyte)datos.NuevoEstado;
                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Estado actualizado", nuevoEstado = usuario.EstadoPresencia });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al actualizar presencia: " + ex.Message });
            }
        }

        // POST: api/auth/registrar
        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar([FromBody] Usuario nuevoUsuario)
        {
            try
            {
                var existeCorreo = await _context.Usuarios.AnyAsync(u => u.Correo == nuevoUsuario.Correo);
                if (existeCorreo)
                {
                    return BadRequest(new { mensaje = "El correo electrónico ya está registrado." });
                }

                if (!CumplePoliticaPassword(nuevoUsuario.Contraseña))
                {
                    return BadRequest(new { mensaje = PasswordPolicyMessage });
                }

                var random = new Random();
                nuevoUsuario.CodigoVerificacion = random.Next(100000, 999999).ToString();
                nuevoUsuario.EsVerificado = false;

                nuevoUsuario.Contraseña = BCrypt.Net.BCrypt.HashPassword(nuevoUsuario.Contraseña);

                nuevoUsuario.FechaRegistro = DateTime.Now;
                if (string.IsNullOrEmpty(nuevoUsuario.Rol)) nuevoUsuario.Rol = "Usuario";
                if (nuevoUsuario.Calificacion == null || nuevoUsuario.Calificacion == 0) nuevoUsuario.Calificacion = 5.00m;
                nuevoUsuario.Estado = 1;

                _context.Usuarios.Add(nuevoUsuario);
                await _context.SaveChangesAsync();

                _ = Task.Run(async () => {
                    try {
                        await _emailService.EnviarCodigoVerificacionAsync(nuevoUsuario.Correo, nuevoUsuario.Nombre, nuevoUsuario.CodigoVerificacion);
                    } catch (Exception ex) {
                        Console.WriteLine("Error enviando mail en segundo plano: " + ex.Message);
                    }
                });

                return Ok(new { mensaje = "Usuario registrado exitosamente. Por favor, revisa tu correo." });
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
                var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == datosVerificacion.Correo);

                if (usuario == null)
                {
                    return NotFound(new { mensaje = "Usuario no encontrado." });
                }

                if (usuario.CodigoVerificacion == datosVerificacion.Codigo)
                {
                    usuario.EsVerificado = true;
                    usuario.CodigoVerificacion = null;
                    await _context.SaveChangesAsync();
                    return Ok(new { mensaje = "Cuenta verificada con éxito." });
                }
                else
                {
                    return BadRequest(new { mensaje = "Código de verificación incorrecto." });
                }
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
                if (string.IsNullOrEmpty(request.Correo))
                {
                    return BadRequest(new { mensaje = "El correo es obligatorio." });
                }

                var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == request.Correo);
                if (usuario == null)
                {
                    return NotFound(new { mensaje = "El correo no está registrado en POKET." });
                }

                var random = new Random();
                usuario.CodigoRecuperacion = random.Next(100000, 999999).ToString();
                await _context.SaveChangesAsync();

                _ = Task.Run(async () => {
                    try {
                        await _emailService.EnviarCodigoRecuperacionAsync(usuario.Correo, usuario.Nombre, usuario.CodigoRecuperacion);
                    } catch (Exception ex) {
                        Console.WriteLine("Error enviando recovery mail: " + ex.Message);
                    }
                });

                return Ok(new { mensaje = "Código enviado. Revisa tu bandeja de entrada." });
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
                var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == datos.Correo);
                
                if (usuario == null)
                {
                    return BadRequest(new { mensaje = "El correo no está registrado." });
                }

                if (usuario.CodigoRecuperacion != datos.Codigo)
                {
                    return BadRequest(new { mensaje = "Código de recuperación incorrecto." });
                }

                return Ok(new { mensaje = "Código correcto. Puedes cambiar tu contraseña." });
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
                var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == datos.Correo);
                
                if (usuario == null)
                {
                    return BadRequest(new { mensaje = "El usuario no existe." });
                }

                if (usuario.CodigoRecuperacion != datos.Codigo)
                {
                    return BadRequest(new { mensaje = "El código es incorrecto o ha expirado." });
                }

                if (!CumplePoliticaPassword(datos.NuevaPassword))
                {
                    return BadRequest(new { mensaje = PasswordPolicyMessage });
                }

                usuario.Contraseña = BCrypt.Net.BCrypt.HashPassword(datos.NuevaPassword);
                usuario.CodigoRecuperacion = null;
                
                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Contraseña actualizada exitosamente." });
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
                var usuarioExistente = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == datosLogin.Correo);
                if(usuarioExistente == null)
                {
                    return Unauthorized(new {mensaje = "Correo o Contraseña incorrectas."});
                }

                if (!usuarioExistente.EsVerificado)
                {
                    return Unauthorized(new { mensaje = "Por favor, verifica tu cuenta a través de tu correo electrónico antes de iniciar sesión.", requiereVerificacion = true });
                }

                bool claveCorrecta = BCrypt.Net.BCrypt.Verify(datosLogin.Contraseña, usuarioExistente.Contraseña);
                if (!claveCorrecta)
                {
                    return Unauthorized(new {mensaje = "Correo o Contraseña incorrectas."});
                }

                return Ok(new
                {
                    mensaje = "Inicio de sesión exitoso.",
                    usuario = new
                    {
                        id = usuarioExistente.IdUsuarios,
                        nombre = usuarioExistente.Nombre,
                        apellido = usuarioExistente.Apellido,
                        correo = usuarioExistente.Correo,
                        telefono = usuarioExistente.Telefono,
                        rol = usuarioExistente.Rol,
                        foto = usuarioExistente.ImgPerfil,
                        estadoPresencia = usuarioExistente.EstadoPresencia,
                        bio = usuarioExistente.Descripcion
                    }
                });
            }
            catch(Exception ex)
            {
                return StatusCode(500, new {mensaje = $"Error Interno: {ex.Message}"});
            }
        }
        [HttpPost("actualizar-perfil")]
        public async Task<IActionResult> ActualizarPerfil([FromBody] ActualizarPerfilDto datos)
        {
            try
            {
                var usuario = await _context.Usuarios.FindAsync(datos.UsuarioId);
                if (usuario == null) return NotFound(new { mensaje = "Usuario no encontrado" });

                // Actualizar campos permitidos
                usuario.Nombre = datos.Nombre ?? usuario.Nombre;
                usuario.Apellido = datos.Apellido ?? usuario.Apellido;
                usuario.Telefono = datos.Telefono;
                usuario.ImgPerfil = datos.ImgPerfil;
                usuario.Descripcion = datos.Bio;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    mensaje = "Perfil actualizado correctamente",
                    usuario = new
                    {
                        id = usuario.IdUsuarios,
                        nombre = usuario.Nombre,
                        apellido = usuario.Apellido,
                        correo = usuario.Correo,
                        rol = usuario.Rol,
                        foto = usuario.ImgPerfil,
                        telefono = usuario.Telefono,
                        estadoPresencia = usuario.EstadoPresencia,
                        bio = usuario.Descripcion
                    }
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
                var usuario = await _context.Usuarios.FindAsync(datos.UsuarioId);
                if (usuario == null) return NotFound(new { mensaje = "Usuario no encontrado" });

                // Verificar contraseña actual
                bool claveCorrecta = BCrypt.Net.BCrypt.Verify(datos.PasswordActual, usuario.Contraseña);
                if (!claveCorrecta)
                {
                    return BadRequest(new { mensaje = "La contraseña actual es incorrecta." });
                }

                if (!CumplePoliticaPassword(datos.NuevaPassword))
                {
                    return BadRequest(new { mensaje = PasswordPolicyMessage });
                }

                usuario.Contraseña = BCrypt.Net.BCrypt.HashPassword(datos.NuevaPassword);
                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Contraseña actualizada exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al cambiar contraseña: " + ex.Message });
            }
        }

        private static bool CumplePoliticaPassword(string? password)
        {
            return !string.IsNullOrWhiteSpace(password) && PasswordPolicy.IsMatch(password);
        }
    }

    // --- DTOs ---
    public class CambiarPasswordDto
    {
        public int UsuarioId { get; set; }
        public string PasswordActual { get; set; } = null!;
        public string NuevaPassword { get; set; } = null!;
    }
    public class ActualizarPerfilDto
    {
        public int UsuarioId { get; set; }
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? Telefono { get; set; }
        public string? ImgPerfil { get; set; }
        public string? Bio { get; set; }
    }
    public class LoginDto
    {
        public string Correo { get; set; } = null!;
        public string Contraseña { get; set; } = null!;
    }

    public class VerificarDto
    {
        public string Correo { get; set; } = null!;
        public string Codigo { get; set; } = null!;
    }

    public class ResetPasswordDto
    {
        public string Correo { get; set; } = null!;
        public string Codigo { get; set; } = null!;
        public string NuevaPassword { get; set; } = null!;
    }

    public class RecoveryRequestDto
    {
        public string Correo { get; set; } = null!;
    }

    public class CambiarPresenciaDto
    {
        public int UsuarioId { get; set; }
        public int NuevoEstado { get; set; }
    }
}
