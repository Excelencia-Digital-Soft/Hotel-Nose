using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ApiObjetos.Models;

public partial class Cierre
{
    [Key]
    public int CierreId { get; set; }

    public int? UsuarioId { get; set; }

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
    public virtual Usuarios? Usuario { get; set; }
}
