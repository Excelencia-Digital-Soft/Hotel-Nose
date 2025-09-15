using System.Text.Json.Serialization;
using hotel.Models.Identity;

namespace hotel.Models;

public partial class Movimientos
{
    public int MovimientosId { get; set; }

    public int? VisitaId { get; set; }

    public int? PagoId { get; set; }

    public int? HabitacionId { get; set; }

    public DateTime? FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    public int? TotalHoras { get; set; }

    public decimal? TotalFacturado { get; set; }

    [Obsolete("Use UserId instead for ASP.NET Identity")]
    public int? UsuarioId { get; set; }

    public DateTime? FechaRegistro { get; set; }
    public string? Descripcion { get; set; }
    public Egresos? Egreso { get; set; }

    public bool? Anulado { get; set; }
    public int InstitucionID { get; set; }
    
    // ASP.NET Identity User tracking
    public string? UserId { get; set; }

    // Navigation properties
    [JsonIgnore]
    public virtual ICollection<Consumo> Consumo { get; } = new List<Consumo>();

    [JsonIgnore]
    public virtual Habitaciones? Habitacion { get; set; }

    public virtual ICollection<MovimientosServicios> MovimientosServicios { get; } =
        new List<MovimientosServicios>();

    public virtual ICollection<MovimientosStock> MovimientosStock { get; } =
        new List<MovimientosStock>();

    [JsonIgnore]
    public virtual Pagos? Pago { get; set; }

    [JsonIgnore]
    public virtual Visitas? Visita { get; set; }
    
    public virtual ApplicationUser? User { get; set; }
}
