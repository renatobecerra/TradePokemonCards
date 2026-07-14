namespace Backend.DTOs
{
    public class EnviarMensajeDto
    {
        public int IdRemitente { get; set; }
        public int IdDestinatario { get; set; }
        public string Texto { get; set; } = null!;
        public int? IdItem { get; set; }
    }
}
