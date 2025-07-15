using System.Text.Json.Serialization;
using hotel.Models.Identity;

namespace hotel.Models;

public partial class Articulos
{
    public int ArticuloId { get; set; }

    public string? NombreArticulo { get; set; }

    public decimal Precio { get; set; }

    public int? UsuarioId { get; set; } // Legacy field - mantener para compatibilidad

    public DateTime? FechaRegistro { get; set; }

    public bool? Anulado { get; set; }

    public int? imagenID { get; set; }
    public Imagenes Imagen { get; set; } = null!;
    public int? CategoriaID { get; set; }
    public int InstitucionID { get; set; }
    
    // Audit fields with AspNetUsers
    public string? CreadoPorId { get; set; }
    public DateTime? FechaCreacion { get; set; }
    public string? ModificadoPorId { get; set; }
    public DateTime? FechaModificacion { get; set; }
    
    // Navigation properties for audit
    [JsonIgnore]
    public virtual ApplicationUser? CreadoPor { get; set; }
    
    [JsonIgnore]
    public virtual ApplicationUser? ModificadoPor { get; set; }

    [JsonIgnore]
    public virtual ICollection<Consumo> Consumo { get; } = new List<Consumo>();

    [JsonIgnore]
    public virtual InventarioInicial? InventarioInicial { get; set; }

    [JsonIgnore]
    public virtual ICollection<Inventarios> Inventarios { get; } = new List<Inventarios>();

    [JsonIgnore]
    public virtual ICollection<InventarioGeneral> InventarioGeneral { get; } =
        new List<InventarioGeneral>();

    [JsonIgnore]
    public virtual ICollection<Encargos> Encargos { get; } = new List<Encargos>();

    [JsonIgnore]
    public virtual ICollection<MovimientosStock> MovimientosStock { get; } =
        new List<MovimientosStock>();
}
