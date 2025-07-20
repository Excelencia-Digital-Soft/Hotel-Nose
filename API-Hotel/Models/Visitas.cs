using System.Text.Json.Serialization;
using hotel.Models.Identity;

namespace hotel.Models;

public partial class Visitas
{
    public int VisitaId { get; set; }

    public string? PatenteVehiculo { get; set; }

    public string? Identificador { get; set; }

    public string? NumeroTelefono { get; set; }

    public DateTime? FechaPrimerIngreso { get; set; }

    [Obsolete]
    public int? UsuarioId { get; set; } // Legacy

    public string? UserId { get; set; } // New with identity

    public DateTime? FechaRegistro { get; set; }

    public bool Anulado { get; set; }
    public int InstitucionID { get; set; }
    public int? HabitacionId { get; set; }

    public virtual ICollection<Movimientos> Movimientos { get; } = new List<Movimientos>();

    [JsonIgnore]
    public virtual ICollection<Encargos> Encargos { get; } = new List<Encargos>();

    public virtual ICollection<Reservas> Reservas { get; } = new List<Reservas>();

    // Navigation properties
    public virtual Habitaciones? Habitacion { get; set; }
    public virtual ApplicationUser? Usuario { get; set; }
    public virtual Institucion? Institucion { get; set; }
    
    // Computed properties
    public Reservas ReservaActiva => Reservas?.FirstOrDefault(r => r.FechaFin == null) ?? default!;
}
