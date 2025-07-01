using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace hotel.Models;

public partial class Inventarios
{
    public int InventarioId { get; set; }

    public int? HabitacionId { get; set; }

    public int? ArticuloId { get; set; }

    public int? Cantidad { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public bool? Anulado { get; set; }
    public int InstitucionID { get; set; }
    public virtual Articulos? Articulo { get; set; }
    [JsonIgnore]
    public virtual Habitaciones? Habitacion { get; set; }
}
