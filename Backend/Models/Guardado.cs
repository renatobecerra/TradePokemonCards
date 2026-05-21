using System;
using System.Collections.Generic;

namespace Backend.Models;

public partial class Guardado
{
    public int IdLista { get; set; }

    public int IdUsuario { get; set; }

    public int IdItem { get; set; }

    public DateTime? FechaGuardado { get; set; }

    public virtual Inventario IdItemNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
