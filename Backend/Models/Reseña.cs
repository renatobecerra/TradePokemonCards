using System;
using System.Collections.Generic;

namespace Backend.Models;

public partial class Reseña
{
    public int ReseñaId { get; set; }

    public int IdUsuario { get; set; }

    public int IdItem { get; set; }

    public string? Texto { get; set; }

    public DateTime? Fecha { get; set; }

    public virtual Inventario IdItemNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
