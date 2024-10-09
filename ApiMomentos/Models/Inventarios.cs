using System;
using System.Collections.Generic;

namespace ApiObjetos.Models;

public partial class Inventarios
{
    public int InventarioId { get; set; }

    public int? HabitacionId { get; set; }

    public int? ArticuloId { get; set; }

    public int? Cantidad { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public bool? Anulado { get; set; }

    public virtual Articulos? Articulo { get; set; }

    public virtual Habitaciones? Habitacion { get; set; }
}
