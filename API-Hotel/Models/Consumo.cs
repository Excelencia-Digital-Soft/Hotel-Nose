using System.Text.Json.Serialization;

namespace hotel.Models;

public partial class Consumo
{
    public int ConsumoId { get; set; }

    public int? MovimientosId { get; set; }

    public int? ArticuloId { get; set; }

    public int? Cantidad { get; set; }

    public decimal? PrecioUnitario { get; set; }
    public bool? EsHabitacion { get; set; }

    public bool? Anulado { get; set; }

    [JsonIgnore]
    public virtual Articulos? Articulo { get; set; }

    [JsonIgnore]
    public virtual Movimientos? Movimientos { get; set; }
}
