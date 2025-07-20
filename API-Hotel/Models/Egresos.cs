using System.Text.Json.Serialization;

namespace hotel.Models;

public partial class Egresos
{
    public int EgresoId { get; set; }

    public int? TipoEgresoId { get; set; }

    public int Cantidad { get; set; }

    public decimal Precio { get; set; }

    public DateTime? Fecha { get; set; }

    public int? MovimientoId { get; set; }

    public int InstitucionID { get; set; }
    public int? CierreID { get; set; }

    [JsonIgnore]
    public virtual TipoEgreso TipoEgreso { get; set; } = null!;

    [JsonIgnore]
    public virtual Movimientos Movimiento { get; set; } = null!;
}
