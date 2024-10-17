using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ApiObjetos.Models;

public partial class Articulos
{
    public int ArticuloId { get; set; }

    public string? NombreArticulo { get; set; }

    public decimal Precio { get; set; }

    public int? UsuarioId { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public bool? Anulado { get; set; }
    [JsonIgnore]
    public virtual ICollection<Consumo> Consumo { get; } = new List<Consumo>();
    [JsonIgnore]
    public virtual InventarioInicial? InventarioInicial { get; set; }
    [JsonIgnore]
    public virtual ICollection<Inventarios> Inventarios { get; } = new List<Inventarios>();
    [JsonIgnore]
    public virtual ICollection<MovimientosStock> MovimientosStock { get; } = new List<MovimientosStock>();
}
