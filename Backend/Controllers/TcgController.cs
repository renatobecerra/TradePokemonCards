using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/tcg")]
    public class TcgController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly PokemonMarketContext _context;

        public TcgController(IHttpClientFactory httpClientFactory, PokemonMarketContext context)
        {
            _httpClient = httpClientFactory.CreateClient();
            _context = context;
        }

        [HttpGet("cartas")]
        public async Task<IActionResult> ObtenerCartas([FromQuery] string? nombre, [FromQuery] string? rareza, [FromQuery] string? set, [FromQuery] int page = 1)
        {
            try
            {
                List<TcgCardDto>? cards = null;

                if (!string.IsNullOrEmpty(set))
                {
                    try
                    {
                        var setResponse = await _httpClient.GetFromJsonAsync<SetResponse>($"https://api.tcgdex.net/v2/es/sets/{set}");
                        cards = setResponse?.Cards;
                    }
                    catch (Exception) {}

                    if (cards == null || cards.Count == 0)
                    {
                        var setResponseEn = await _httpClient.GetFromJsonAsync<SetResponse>($"https://api.tcgdex.net/v2/en/sets/{set}");
                        cards = setResponseEn?.Cards;
                    }
                }
                else if (!string.IsNullOrEmpty(rareza))
                {
                    try
                    {
                        var rarityResponse = await _httpClient.GetFromJsonAsync<RarityResponse>($"https://api.tcgdex.net/v2/es/rarities/{rareza}");
                        cards = rarityResponse?.Cards;
                    }
                    catch (Exception) {}

                    if (cards == null || cards.Count == 0)
                    {
                        var rarityResponseEn = await _httpClient.GetFromJsonAsync<RarityResponse>($"https://api.tcgdex.net/v2/en/rarities/{rareza}");
                        cards = rarityResponseEn?.Cards;
                    }
                }
                else
                {
                    try
                    {
                        cards = await _httpClient.GetFromJsonAsync<List<TcgCardDto>>("https://api.tcgdex.net/v2/es/cards");
                    }
                    catch (Exception) {}

                    if (cards == null || cards.Count == 0)
                    {
                        cards = await _httpClient.GetFromJsonAsync<List<TcgCardDto>>("https://api.tcgdex.net/v2/en/cards");
                    }
                }
                
                if (cards == null) return Ok(new List<TcgCardDto>());

                var query = cards.AsEnumerable();
                if (!string.IsNullOrEmpty(nombre))
                {
                    query = query.Where(c => c.Name.Contains(nombre, StringComparison.OrdinalIgnoreCase));
                }

                var list = query
                    .Skip((page - 1) * 30)
                    .Take(30)
                    .ToList();
                var cardIds = list.Select(c => c.Id).ToList();

                
                var dbPrices = await _context.InventarioUsuarios
                    .Include(i => i.IdItemNavigation)
                    .Where(i => cardIds.Contains(i.IdItemNavigation.id_tgc) && i.IdItemNavigation.precio != null && i.IdItemNavigation.precio > 0)
                    .Select(i => new { IdTgc = i.IdItemNavigation.id_tgc, Precio = i.IdItemNavigation.precio.Value })
                    .ToListAsync();

                var averages = dbPrices
                    .GroupBy(p => p.IdTgc)
                    .ToDictionary(g => g.Key, g => g.Average(x => x.Precio));

                foreach (var card in list)
                {
                    if (averages.TryGetValue(card.Id, out var avgPrice))
                    {
                        if (card.Pricing == null) card.Pricing = new CardPricing();
                        if (card.Pricing.Tcgplayer == null) card.Pricing.Tcgplayer = new MarketPricing();
                        card.Pricing.Tcgplayer.Market = avgPrice / 950.0;
                        if (card.Pricing.Cardmarket != null) card.Pricing.Cardmarket.Avg = null;
                    }
                }

                return Ok(list);
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
                TcgCardDto? card = null;
                try
                {
                    card = await _httpClient.GetFromJsonAsync<TcgCardDto>($"https://api.tcgdex.net/v2/es/cards/{id}");
                }
                catch (Exception) {}

                if (card == null)
                {
                    card = await _httpClient.GetFromJsonAsync<TcgCardDto>($"https://api.tcgdex.net/v2/en/cards/{id}");
                }

                if (card == null) return NotFound();

                
                var precios = await _context.InventarioUsuarios
                    .Include(i => i.IdItemNavigation)
                    .Where(i => i.IdItemNavigation.id_tgc == id && i.IdItemNavigation.precio != null && i.IdItemNavigation.precio > 0)
                    .Select(i => i.IdItemNavigation.precio.Value)
                    .ToListAsync();

                if (precios.Count > 0)
                {
                    double avgPrice = precios.Average();
                    if (card.Pricing == null) card.Pricing = new CardPricing();
                    if (card.Pricing.Tcgplayer == null) card.Pricing.Tcgplayer = new MarketPricing();
                    card.Pricing.Tcgplayer.Market = avgPrice / 950.0;
                    if (card.Pricing.Cardmarket != null) card.Pricing.Cardmarket.Avg = null;
                }

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

        private class SetResponse
        {
            public string Name { get; set; } = null!;
            public List<TcgCardDto> Cards { get; set; } = null!;
        }
    }
}
