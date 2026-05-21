using System;
using System.Collections.Generic;

namespace Backend.Models;

public partial class TopRegistro
{
    public int IdTop { get; set; }

    public int IdItem { get; set; }

    public int? Posicion { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public virtual Inventario IdItemNavigation { get; set; } = null!;
}
