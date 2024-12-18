using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ApiObjetos.Models;

public partial class Pagos
{
    public int? PagoId { get; set; }

    public decimal? MontoEfectivo { get; set; }

    public decimal? MontoBillVirt { get; set; }

    public decimal? MontoTarjeta { get; set; }

    public decimal? MontoDescuento { get; set; }

    public int? MedioPagoId { get; set; }

    public int? CierreId { get; set; }
    public DateTime? fechaHora { get; set; }    
    public string? Observacion { get; set; }
    public virtual Cierre? Cierre { get; set; }
    [JsonIgnore]
    public virtual MediosPago? MedioPago { get; set; }
    [JsonIgnore]
    public virtual ICollection<Movimientos> Movimientos { get; } = new List<Movimientos>();
}
