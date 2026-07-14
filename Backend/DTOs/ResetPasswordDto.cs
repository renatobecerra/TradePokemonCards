namespace Backend.DTOs
{
    public class ResetPasswordDto
    {
        public string Correo { get; set; } = null!;
        public string Codigo { get; set; } = null!;
        public string NuevaPassword { get; set; } = null!;
    }
}
