using System;
using System.Collections.Generic;

namespace hotel.Models;

public partial class TipoTarifa
{
    public int TipoTarifaId { get; set; }

    public string? NombreTipoTarifa { get; set; }

    public int? UsuarioId { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public bool? Anulado { get; set; }

    public virtual ICollection<Tarifas> Tarifas { get; } = new List<Tarifas>();
}
