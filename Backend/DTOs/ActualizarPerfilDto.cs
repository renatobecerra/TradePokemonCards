namespace Backend.DTOs
{
    public class ActualizarPerfilDto
    {
        public int UsuarioId { get; set; }
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? Telefono { get; set; }
        public string? Bio { get; set; }
        public string? ImgPerfil { get; set; }
    }
}
