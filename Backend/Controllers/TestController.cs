using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Models; // Asegúrate de que apunte a tu carpeta de modelos

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/Test")]
    public class TestController : ControllerBase
    {
        private readonly PokemonMarketContext _context;

        public TestController(PokemonMarketContext context)
        {
            _context = context;
        }

        // Endpoint para probar la conexión
        [HttpGet("usuarios")]
        public async Task<IActionResult> GetUsuarios()
        {
            try
            {
                // Intenta traer los usuarios de la base de datos
                var usuarios = await _context.Usuarios.ToListAsync();
                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                // Si la base de datos o la conexión fallan, saltará aquí
                return StatusCode(500, $"Error de conexión: {ex.Message}");
            }
        }
    }
}