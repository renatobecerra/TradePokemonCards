using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend.Models;

public partial class Transaccion
{
    [Key]
    public int IdTransaccion { get; set; }

    public int IdVendedor { get; set; }
    public int IdComprador { get; set; }
    
    // Identifies the specific item in the seller's inventory
    public int IdInventarioUser { get; set; }

    public int? Precio { get; set; }

    public DateTime Fecha { get; set; }

    public string Estado { get; set; } = null!; // Pendiente, Completado, Cancelado

    // Navigation properties
    [ForeignKey("IdVendedor")]
    [JsonIgnore]
    public virtual Usuario? VendedorNavigation { get; set; }

    [ForeignKey("IdComprador")]
    [JsonIgnore]
    public virtual Usuario? CompradorNavigation { get; set; }

    [ForeignKey("IdInventarioUser")]
    [JsonIgnore]
    public virtual InventarioUsuario? InventarioUserNavigation { get; set; }
}
