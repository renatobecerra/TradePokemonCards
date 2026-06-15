using System;
using System.Collections.Generic;

namespace Backend.Models;

public partial class Inventario
{
    public int IdItem { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Rareza { get; set; }

    public string? Edicion { get; set; }

    public string? ImgLink { get; set; }

    public int? precio {get; set; }

    public string? id_tgc {get; set; }

    public virtual ICollection<Guardado> Guardados { get; set; } = new List<Guardado>();

    public virtual ICollection<InventarioUsuario> InventarioUsuarios { get; set; } = new List<InventarioUsuario>();

    public virtual ICollection<Mensaje> Mensajes { get; set; } = new List<Mensaje>();

    public virtual ICollection<Reseña> Reseñas { get; set; } = new List<Reseña>();

    public virtual ICollection<TopRegistro> TopRegistros { get; set; } = new List<TopRegistro>();
}
