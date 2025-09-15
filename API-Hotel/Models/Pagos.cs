using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using hotel.Models.Identity;

namespace hotel.Models;

public partial class Pagos
{
    public int PagoId { get; set; }

    public decimal? MontoEfectivo { get; set; }

    public decimal? MontoBillVirt { get; set; }

    public decimal? MontoTarjeta { get; set; }

    public decimal? Adicional { get; set; }

    public decimal? MontoDescuento { get; set; }

    public int? MedioPagoId { get; set; }

    public int? CierreId { get; set; }

    public int? TarjetaId { get; set; }
    public DateTime? fechaHora { get; set; }    
    public string? Observacion { get; set; }
    public Recargos? Recargo { get; set; }
    public int InstitucionID { get; set; }
    
    // ASP.NET Identity User tracking
    public string? UserId { get; set; }
    
    // Navigation properties
    public virtual Cierre? Cierre { get; set; }
    [JsonIgnore]
    public virtual MediosPago? MedioPago { get; set; }
    [JsonIgnore]
    public virtual ICollection<Movimientos> Movimientos { get; } = new List<Movimientos>();
    public virtual ApplicationUser? User { get; set; }
}
