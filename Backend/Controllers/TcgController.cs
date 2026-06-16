using Microsoft.AspNetCore.Mvc;
using Backend.Models;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/tcg")]
    public class TcgController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public TcgController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        [HttpGet("cartas")]
        public async Task<IActionResult> ObtenerCartas([FromQuery] string? nombre, [FromQuery] string? rareza)
        {
            try
            {
                List<TcgCardDto>? cards;

                if (!string.IsNullOrEmpty(rareza))
                {
                    // Si hay rareza, usamos el endpoint específico de TCGdex para esa rareza
                    var rarityResponse = await _httpClient.GetFromJsonAsync<RarityResponse>($"https://api.tcgdex.net/v2/es/rarities/{rareza}");
                    cards = rarityResponse?.Cards;
                }
                else
                {
                    // Si no, cargamos el catálogo general
                    cards = await _httpClient.GetFromJsonAsync<List<TcgCardDto>>("https://api.tcgdex.net/v2/es/cards");
                }
                
                if (cards == null) return Ok(new List<TcgCardDto>());

                var query = cards.AsEnumerable();
                if (!string.IsNullOrEmpty(nombre))
                {
                    query = query.Where(c => c.Name.Contains(nombre, StringComparison.OrdinalIgnoreCase));
                }

                return Ok(query.Take(30).ToList());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener cartas de TCGdex", detalle = ex.Message });
            }
        }

        [HttpGet("cartas/{id}")]
        public async Task<IActionResult> ObtenerDetallesCarta(string id)
        {
            try
            {
                var card = await _httpClient.GetFromJsonAsync<TcgCardDto>($"https://api.tcgdex.net/v2/es/cards/{id}");
                if (card == null) return NotFound();
                return Ok(card);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener detalles de la carta", detalle = ex.Message });
            }
        }

        private class RarityResponse
        {
            public string Name { get; set; } = null!;
            public List<TcgCardDto> Cards { get; set; } = null!;
        }
    }
}
