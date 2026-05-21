using System;
using System.Collections.Generic;

namespace Backend.Models;

public partial class Usuario
{
    public int IdUsuarios { get; set; }

    public string Nombre { get; set; } = null!;

    public string Contraseña { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public string? Telefono { get; set; }

    public sbyte? Estado { get; set; }

    public string? Rol { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public decimal? Calificacion { get; set; }

    public string? ImgPerfil { get; set; }

    public virtual ICollection<Guardado> Guardados { get; set; } = new List<Guardado>();

    public virtual ICollection<Inventario> Inventarios { get; set; } = new List<Inventario>();

    public virtual ICollection<Mensaje> MensajeIdDestinatarioNavigations { get; set; } = new List<Mensaje>();

    public virtual ICollection<Mensaje> MensajeIdRemitenteNavigations { get; set; } = new List<Mensaje>();

    public virtual ICollection<Notificacione> Notificaciones { get; set; } = new List<Notificacione>();

    public virtual ICollection<Reseña> Reseñas { get; set; } = new List<Reseña>();
}
