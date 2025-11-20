using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace hotel.Models;

/// <summary>
/// Legacy general inventory table - maintained for backward compatibility
/// Stores general (non-location-specific) inventory quantities
/// </summary>
[Table("InventarioGeneral")]
public partial class InventarioGeneral
{
    [Key]
    [Column("InventarioID")]
    public int InventarioId { get; set; }

    [Column("ArticuloID")]
    public int? ArticuloId { get; set; }

    [Column("Cantidad")]
    public int? Cantidad { get; set; }

    [Column("FechaRegistro")]
    public DateTime? FechaRegistro { get; set; }

    [Column("Anulado")]
    public bool? Anulado { get; set; }

    [Required]
    [Column("InstitucionID")]
    public int InstitucionID { get; set; }

    // Navigation properties
    [ForeignKey("ArticuloId")]
    [JsonIgnore] // Evita ciclos de serialización
    public virtual Articulos? Articulo { get; set; }

    [ForeignKey("InstitucionID")]
    [JsonIgnore]
    public virtual Institucion? Institucion { get; set; }
}