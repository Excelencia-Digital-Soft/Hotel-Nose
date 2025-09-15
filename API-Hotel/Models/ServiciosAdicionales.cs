using System;
using System.Collections.Generic;

namespace hotel.Models;

public partial class ServiciosAdicionales
{
    public int ServicioId { get; set; }

    public string? NombreServicio { get; set; }

    public decimal? Precio { get; set; }

    public int? UsuarioId { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public bool? Anulado { get; set; }

    public virtual ICollection<MovimientosServicios> MovimientosServicios { get; } = new List<MovimientosServicios>();
}
