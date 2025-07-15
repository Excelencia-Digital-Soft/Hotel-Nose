using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using hotel.Models.Identity;

namespace hotel.Models;

public partial class CategoriasArticulos
{
    public int CategoriaId { get; set; }

    public string? NombreCategoria { get; set; }

    public bool? Anulado { get; set; }
    public int InstitucionID { get; set; }
    
    // Image support
    public int? imagenID { get; set; }
    public virtual Imagenes? Imagen { get; set; }
    
    // Audit fields
    public string? CreadoPorId { get; set; }
    public DateTime? FechaCreacion { get; set; }
    public DateTime? FechaRegistro { get; set; }
    public string? ModificadoPorId { get; set; }
    public DateTime? FechaModificacion { get; set; }
    
    // Navigation properties for audit
    [JsonIgnore]
    public virtual ApplicationUser? CreadoPor { get; set; }
    [JsonIgnore]
    public virtual ApplicationUser? ModificadoPor { get; set; }
}
