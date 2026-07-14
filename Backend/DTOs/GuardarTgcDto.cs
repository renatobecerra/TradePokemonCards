namespace Backend.DTOs
{
    public class GuardarTgcDto
    {
        public int IdUsuario { get; set; }
        public string IdTgc { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string? Rareza { get; set; }
        public string? Edicion { get; set; }
        public string? ImgLink { get; set; }
        public int? Precio { get; set; }
    }
}
