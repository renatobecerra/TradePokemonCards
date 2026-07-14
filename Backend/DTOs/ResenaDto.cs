namespace Backend.DTOs
{
    public class ResenaDto
    {
        public int IdUsuarioResenador { get; set; }
        public int? IdUsuarioResenado { get; set; }
        public int? IdItem { get; set; }
        public int Calificacion { get; set; }
        public string Texto { get; set; } = null!;
    }
}
