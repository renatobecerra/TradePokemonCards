using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend.Models;

public partial class Reseña
{
    public int ReseñaId { get; set; }

    public int IdUsuarioReseñador { get; set; }

    public int? IdUsuarioReseñado { get; set; }

    public int? IdItem { get; set; }

    public int Calificacion { get; set; }

    public string? Texto { get; set; }

    public DateTime? Fecha { get; set; }

    [JsonIgnore]
    public virtual Inventario? IdItemNavigation { get; set; }

    [JsonIgnore]
    public virtual Usuario? IdUsuarioReseñadorNavigation { get; set; }

    [JsonIgnore]
    public virtual Usuario? IdUsuarioReseñadoNavigation { get; set; }
}
