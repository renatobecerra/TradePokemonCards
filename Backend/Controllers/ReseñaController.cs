using Microsoft.AspNetCore.Mvc;
using Backend.Services.Interfaces;
using Backend.DTOs;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResenaController : ControllerBase
    {
        private readonly IResenaService _resenaService;

        public ResenaController(IResenaService resenaService)
        {
            _resenaService = resenaService;
        }

        
        [HttpGet("usuario/{id}")]
        public async Task<IActionResult> GetResenasPorUsuario(int id)
        {
            var resenas = await _resenaService.GetResenasPorUsuarioAsync(id);
            return Ok(resenas);
        }

        
        [HttpGet("carta/{id}")]
        public async Task<IActionResult> GetResenasPorCarta(int id)
        {
            var resenas = await _resenaService.GetResenasPorCartaAsync(id);
            return Ok(resenas);
        }

        
        [HttpPost]
        public async Task<IActionResult> PostResena([FromBody] ResenaDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (exito, mensaje) = await _resenaService.PostResenaAsync(dto);
            if (!exito)
            {
                return BadRequest(new { message = mensaje });
            }

            return Ok(new { message = mensaje });
        }
    }
}
