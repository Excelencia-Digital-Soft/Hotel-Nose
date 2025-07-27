using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using hotel.Models.Identity;

namespace hotel.Models;

public partial class Cierre
{
    [Key]
    public int CierreId { get; set; }

    public int? UsuarioId { get; set; } // Legacy field - keep for compatibility
    
    /// <summary>
    /// User ID from AspNetUsers (new field)
    /// </summary>
    public string? UserId { get; set; }

    public DateTime? FechaHoraCierre { get; set; }

    public decimal? TotalIngresosEfectivo { get; set; }

    public decimal? TotalIngresosBillVirt { get; set; }

    public decimal? TotalIngresosTarjeta { get; set; }

    public string? Observaciones { get; set; }

    public bool? EstadoCierre { get; set; }

    public decimal? MontoInicialCaja { get; set; }
    public int InstitucionID { get; set; }

    [JsonIgnore]
    public virtual ICollection<Pagos> Pagos { get; } = new List<Pagos>();

    [JsonIgnore]
    public virtual ICollection<Egresos>? Egresos { get; } = new List<Egresos>();

    [JsonIgnore]
    public virtual Usuarios? Usuario { get; set; } // Legacy navigation property
    
    /// <summary>
    /// User who created/closed this cash register (AspNetUsers)
    /// </summary>
    [JsonIgnore]
    public virtual ApplicationUser? User { get; set; }
}
