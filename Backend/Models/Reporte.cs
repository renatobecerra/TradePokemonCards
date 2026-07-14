using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("reportes")]
    public class Reporte
    {
        [Key]
        [Column("IdReporte")]
        public int IdReporte { get; set; }

        [Column("IdUsuarioReportante")]
        public int IdUsuarioReportante { get; set; }

        [Column("IdUsuarioReportado")]
        public int IdUsuarioReportado { get; set; }

        [Column("Motivo")]
        [StringLength(500)]
        public string Motivo { get; set; } = null!;

        [Column("Fecha")]
        public DateTime Fecha { get; set; }

        [Column("Estado")]
        [StringLength(50)]
        public string Estado { get; set; } = "Pendiente"; // Pendiente, Resuelto
    }
}
