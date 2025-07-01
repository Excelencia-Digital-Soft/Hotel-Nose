using System;
using System.Collections.Generic;

namespace hotel.Models;

public partial class InventarioInicial
{
    public int ArticuloId { get; set; }

    public int? CantidadInicial { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public bool? Anulado { get; set; }

    public virtual Articulos Articulo { get; set; } = null!;
}
