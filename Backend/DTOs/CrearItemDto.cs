namespace Backend.DTOs
{
    public class CrearItemDto
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; } = null!;
        public string? Estado { get; set; }
        public string? Rareza { get; set; }
        public string? Edicion { get; set; }
        public string? ImgLink { get; set; }
        public string? IdTgc { get; set; }
        public decimal? Precio { get; set; }
        public int? Cantidad { get; set; }
    }
}
