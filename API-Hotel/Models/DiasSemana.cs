using System;
using System.Collections.Generic;

namespace hotel.Models;

public partial class DiasSemana
{
    public int DiaSemanaId { get; set; }

    public string? NombreDiaSemana { get; set; }

    public virtual ICollection<Tarifas> Tarifas { get; } = new List<Tarifas>();
}
