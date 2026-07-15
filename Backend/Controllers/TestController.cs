using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Models; 

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

        
        [HttpGet("usuarios")]
        public async Task<IActionResult> GetUsuarios()
        {
            try
            {
                
                var usuarios = await _context.Usuarios.ToListAsync();
                return Ok(usuarios);
            }
            catch (Exception ex)
            {
               
                return StatusCode(500, $"Error de conexión: {ex.Message}");
            }
        }
    }
}