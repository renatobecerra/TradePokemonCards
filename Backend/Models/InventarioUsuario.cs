using System;
using System.Collections.Generic;

namespace Backend.Models;

public partial class InventarioUsuario
{
    public int IdInventarioUser { get; set; }

    public int IdUsuario { get; set; }

    public int IdItem { get; set; }

    public string? EstadoFisico { get; set; }

    public int? Cantidad { get; set; }

    public DateTime? FechaObtencion { get; set; }

    public virtual Inventario IdItemNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
