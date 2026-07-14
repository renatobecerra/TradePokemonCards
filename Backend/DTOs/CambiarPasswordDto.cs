namespace Backend.DTOs
{
    public class CambiarPasswordDto
    {
        public int UsuarioId { get; set; }
        public string PasswordActual { get; set; } = null!;
        public string NuevaPassword { get; set; } = null!;
    }
}
