using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using BCrypt.Net;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly PokemonMarketContext _context;

        public AuthController(PokemonMarketContext context)
        {
            _context = context;
        }

        // POST: api/auth/registrar
        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar([FromBody] Usuario nuevoUsuario)
        {
            try
            {
                // 1. Validar si el correo ya existe en la base de datos
                var existeCorreo = await _context.Usuarios.AnyAsync(u => u.Correo == nuevoUsuario.Correo);
                if (existeCorreo)
                {
                    return BadRequest(new { mensaje = "El correo electrónico ya está registrado." });
                }

                // 2. Encriptar la contraseña usando BCrypt antes de guardar
                nuevoUsuario.Contraseña = BCrypt.Net.BCrypt.HashPassword(nuevoUsuario.Contraseña);

                // 3. Asignar valores por defecto para un usuario nuevo si vienen vacíos
                nuevoUsuario.FechaRegistro = DateTime.Now;
                if (string.IsNullOrEmpty(nuevoUsuario.Rol)) nuevoUsuario.Rol = "Usuario";
                if (nuevoUsuario.Calificacion == null || nuevoUsuario.Calificacion == 0) nuevoUsuario.Calificacion = 5.00m; // Nota máxima inicial
                nuevoUsuario.Estado = 1; // Usuario activo

                // 4. Guardar en la base de datos
                _context.Usuarios.Add(nuevoUsuario);
                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Usuario registrado exitosamente de forma segura." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error interno del servidor: {ex.Message}" });
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
                        rol = usuarioExistente.Rol
                    }
                });
            }
            catch(Exception ex)
            {
                return StatusCode(500, new {mensaje = $"Error Interno: {ex.Message}"});
            }
        }
    }
    // Estructura limpia para recibir los datos de inicio de sesión
    public class LoginDto
    {
        public string Correo { get; set; } = null!;
        public string Contraseña { get; set; } = null!;
    }
}