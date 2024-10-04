using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ApiObjetos.Models;

public partial class Consumo
{
    public int ConsumoId { get; set; }

    public int? MovimientosId { get; set; }

    public int? ArticuloId { get; set; }

    public int? Cantidad { get; set; }

    public decimal? PrecioUnitario { get; set; }

    public bool? Anulado { get; set; }
    [JsonIgnore]
    public virtual Articulo? Articulo { get; set; }
    [JsonIgnore]
    public virtual Movimiento? Movimientos { get; set; }
}
