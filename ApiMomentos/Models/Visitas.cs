using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

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

    public bool Anulado { get; set; }

    public virtual ICollection<Movimientos> Movimientos { get; } = new List<Movimientos>();
    [JsonIgnore]
    public virtual ICollection<Encargos> Encargos { get; } = new List<Encargos>();
    
    public virtual ICollection<Reservas> Reservas { get; } = new List<Reservas>();
    public Reservas ReservaActiva => Reservas?.FirstOrDefault(r => r.FechaFin == null);


}
