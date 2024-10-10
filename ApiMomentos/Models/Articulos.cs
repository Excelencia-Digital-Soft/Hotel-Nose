using System;
using System.Collections.Generic;

namespace ApiObjetos.Models;

public partial class Articulos
{
    public int ArticuloId { get; set; }

    public string? NombreArticulo { get; set; }

    public decimal Precio { get; set; }

    public int? UsuarioId { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public bool? Anulado { get; set; }

    public virtual ICollection<Consumo> Consumo { get; } = new List<Consumo>();

    public virtual InventarioInicial? InventarioInicial { get; set; }

    public virtual ICollection<Inventarios> Inventarios { get; } = new List<Inventarios>();

    public virtual ICollection<MovimientosStock> MovimientosStock { get; } = new List<MovimientosStock>();
}
