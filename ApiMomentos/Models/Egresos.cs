using System;
using System.Text.Json.Serialization;

namespace ApiObjetos.Models;

public partial class Egresos
{
    public int EgresoId { get; set; }

    public int? TipoEgresoId { get; set; }

    public int Cantidad { get; set; }

    public decimal Precio { get; set; }

    public DateTime? Fecha { get; set; }

    public int? MovimientoId { get; set; }

    [JsonIgnore]
    public virtual TipoEgreso TipoEgreso { get; set; }

    [JsonIgnore]
    public virtual Movimientos Movimiento { get; set; }
}
