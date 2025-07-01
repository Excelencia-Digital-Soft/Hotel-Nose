using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace hotel.Models;

public partial class InventarioGeneral
{
    [Key]
    public int InventarioId { get; set; }

    public int? ArticuloId { get; set; }

    public int? Cantidad { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public bool? Anulado { get; set; }
    public int InstitucionID { get; set; }
    public virtual Articulos? Articulo { get; set; }
}
