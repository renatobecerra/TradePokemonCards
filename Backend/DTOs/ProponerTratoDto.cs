namespace Backend.DTOs
{
    public class ProponerTratoDto
    {
        public int IdVendedor { get; set; }
        public int IdComprador { get; set; }
        public int IdInventarioUser { get; set; }
        public int? Precio { get; set; }
        public int? IdInventarioUserIntercambio { get; set; }
        public int IdProponente { get; set; }
    }
}
