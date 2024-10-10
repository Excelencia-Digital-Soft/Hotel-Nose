using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ApiObjetos.Models;

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

    public int? UsuarioId { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public bool? Anulado { get; set; }
    [JsonIgnore]

    public virtual ICollection<Consumo> Consumo { get; } = new List<Consumo>();
    [JsonIgnore]

    public virtual Habitaciones? Habitacion { get; set; }

    public virtual ICollection<MovimientosServicios> MovimientosServicios { get; } = new List<MovimientosServicios>();

    public virtual ICollection<MovimientosStock> MovimientosStock { get; } = new List<MovimientosStock>();
    [JsonIgnore]

    public virtual Pagos? Pago { get; set; }
    [JsonIgnore]
    public virtual Visitas? Visita { get; set; }
}
