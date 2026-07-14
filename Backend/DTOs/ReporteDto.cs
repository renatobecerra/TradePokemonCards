namespace Backend.DTOs
{
    public class ReporteDto
    {
        public int IdUsuarioReportante { get; set; }
        public int IdUsuarioReportado { get; set; }
        public string Motivo { get; set; } = null!;
    }
}
