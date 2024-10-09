using System;
using System.Collections.Generic;

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

    public virtual ICollection<Consumo> Consumo { get; } = new List<Consumo>();

    public virtual Habitaciones? Habitacion { get; set; }

    public virtual ICollection<MovimientosServicios> MovimientosServicios { get; } = new List<MovimientosServicios>();

    public virtual ICollection<MovimientosStock> MovimientosStock { get; } = new List<MovimientosStock>();

    public virtual Pagos? Pago { get; set; }

    public virtual Visitas? Visita { get; set; }
}
