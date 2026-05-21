using System;
using System.Collections.Generic;

namespace Backend.Models;

public partial class Notificacione
{
    public int IdNotificaciones { get; set; }

    public int IdUserDestinatario { get; set; }

    public string? Asunto { get; set; }

    public string? Contenido { get; set; }

    public bool? Leido { get; set; }

    public DateTime? Fecha { get; set; }

    public virtual Usuario IdUserDestinatarioNavigation { get; set; } = null!;
}
