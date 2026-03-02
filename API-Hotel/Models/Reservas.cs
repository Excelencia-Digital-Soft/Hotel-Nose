using System.Text.Json.Serialization;
using hotel.Models.Identity;

namespace hotel.Models;

public partial class Reservas
{
    public int ReservaId { get; set; }

    public int? VisitaId { get; set; }

    public int? HabitacionId { get; set; }

    public DateTime? FechaReserva { get; set; }

    public DateTime? FechaFin { get; set; }

    public int? TotalHoras { get; set; }
    public int? TotalMinutos { get; set; }
    public int? MovimientoId { get; set; }
    public int? PromocionId { get; set; }

    [Obsolete]
    public int? UsuarioId { get; set; }

    public string? UserId { get; set; }
    public int? PausaHoras { get; set; }
    public int? PausaMinutos { get; set; }
    public DateTime? FechaRegistro { get; set; }
    public int InstitucionID { get; set; }
    public DateTime? FechaAnula { get; set; }

    // Navigation properties
    [JsonIgnore]
    public virtual Visitas? Visita { get; set; }
    public virtual Promociones? Promocion { get; set; }
    public virtual Habitaciones? Habitacion { get; set; }
    public virtual ApplicationUser? Usuario { get; set; }
    public virtual Institucion? Institucion { get; set; }
    public int? CierreId { get; set; }
    public virtual Cierre? Cierre { get; set; }
}
