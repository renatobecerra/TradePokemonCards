using System;
using System.Collections.Generic;

namespace Backend.Models;

public partial class Mensaje
{
    public int IdMensaje { get; set; }

    public int IdRemitente { get; set; }

    public int IdDestinatario { get; set; }

    public int? IdItem { get; set; }

    public string Texto { get; set; } = null!;

    public bool? Estado { get; set; }

    public DateTime? Fecha { get; set; }

    public bool EliminadoPorRemitente { get; set; } = false;

    public bool EliminadoPorDestinatario { get; set; } = false;

    public virtual Usuario IdDestinatarioNavigation { get; set; } = null!;

    public virtual Inventario? IdItemNavigation { get; set; }

    public virtual Usuario IdRemitenteNavigation { get; set; } = null!;
}
