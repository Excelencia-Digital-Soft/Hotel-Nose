using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ApiObjetos.Models;

public partial class Reservas
{
    public int ReservaId { get; set; }

    public int? VisitaId { get; set; }

    public int? HabitacionId { get; set; }

    public DateTime? FechaReserva { get; set; }

    public DateTime? FechaFin { get; set; }

    public int? TotalHoras { get; set; }
    public int? TotalMinutos { get; set; }

    public int? UsuarioId { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public bool? Anulado { get; set; }
    [JsonIgnore]
    public virtual Habitaciones? Habitacion { get; set; }
    [JsonIgnore]
    public virtual Visitas? Visita { get; set; }
}
