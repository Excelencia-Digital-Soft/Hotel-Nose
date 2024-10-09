using System;
using System.Collections.Generic;

namespace ApiObjetos.Models;

public partial class Visitas
{
    public int VisitaId { get; set; }

    public string? PatenteVehiculo { get; set; }

    public string? Identificador { get; set; }

    public string? NumeroTelefono { get; set; }

    public DateTime? FechaPrimerIngreso { get; set; }

    public int? UsuarioId { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public bool? Anulado { get; set; }

    public virtual ICollection<Movimientos> Movimientos { get; } = new List<Movimientos>();

    public virtual ICollection<Reservas> Reservas { get; } = new List<Reservas>();
}
