namespace Backend.Models
{
    public class TcgCardDto
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Image { get; set; }
        public string? Rarity { get; set; }
        public CardPricing? Pricing { get; set; }
    }

    public class CardPricing
    {
        public MarketPricing? Tcgplayer { get; set; }
        public MarketPricing? Cardmarket { get; set; }
    }

    public class MarketPricing
    {
        public double? Low { get; set; }
        public double? Mid { get; set; }
        public double? High { get; set; }
        public double? Market { get; set; }
        public double? Avg { get; set; } // Para cardmarket
    }
}
