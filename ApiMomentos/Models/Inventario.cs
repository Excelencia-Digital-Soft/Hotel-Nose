using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ApiObjetos.Models;

public partial class Inventario
{
    [Key]
    public int InventarioId { get; set; }
    public int ArticuloId { get; set; }
    public int HabitacionId { get; set; }

    public int Cantidad { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public bool? Anulado { get; set; }
    [JsonIgnore]
    public virtual Articulo Articulo { get; set; } = null!;
    [JsonIgnore]
    public virtual Habitaciones Habitacion { get; set; } = null!;

}
