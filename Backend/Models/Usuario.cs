using System;
using System.Collections.Generic;

namespace Backend.Models;

public partial class Usuario
{
    public int IdUsuarios { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Apellido { get; set; }

    public string? CodigoVerificacion { get; set; }

    public bool? EsVerificado { get; set; }

    public string? CodigoRecuperacion { get; set; }

    public string Contraseña { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public string? Telefono { get; set; }

    public sbyte? Estado { get; set; }

    public string? Rol { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public decimal? Calificacion { get; set; }

    public sbyte? EstadoPresencia { get; set; }

    public string? Descripcion { get; set; }

    public string? ImgPerfil { get; set; }

    public string? MotivoBaneo { get; set; }

    public DateTime? FechaDesbaneo { get; set; }

    public virtual ICollection<Guardado> Guardados { get; set; } = new List<Guardado>();

    public virtual ICollection<InventarioUsuario> InventarioUsuarios { get; set; } = new List<InventarioUsuario>();

    public virtual ICollection<Mensaje> MensajeIdDestinatarioNavigations { get; set; } = new List<Mensaje>();

    public virtual ICollection<Mensaje> MensajeIdRemitenteNavigations { get; set; } = new List<Mensaje>();

    public virtual ICollection<Notificacione> Notificaciones { get; set; } = new List<Notificacione>();

    public virtual ICollection<Reseña> ReseñasHechas { get; set; } = new List<Reseña>();

    public virtual ICollection<Reseña> ReseñasRecibidas { get; set; } = new List<Reseña>();
}
