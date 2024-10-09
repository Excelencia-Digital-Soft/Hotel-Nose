using System;
using System.Collections.Generic;

namespace ApiObjetos.Models;

public partial class Habitaciones
{
    public int HabitacionId { get; set; }

    public string? NombreHabitacion { get; set; }

    public int? CategoriaId { get; set; }

    public bool? Disponible { get; set; }

    public DateTime? ProximaReserva { get; set; }

    public int? UsuarioId { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public bool? Anulado { get; set; }

    public virtual CategoriasHabitaciones? Categoria { get; set; }

    public virtual ICollection<HabitacionesVirtuales> HabitacionesVirtualesHabitacion1 { get; } = new List<HabitacionesVirtuales>();

    public virtual ICollection<HabitacionesVirtuales> HabitacionesVirtualesHabitacion2 { get; } = new List<HabitacionesVirtuales>();

    public virtual ICollection<Inventarios> Inventarios { get; } = new List<Inventarios>();

    public virtual ICollection<Movimientos> Movimientos { get; } = new List<Movimientos>();

    public virtual ICollection<Reservas> Reservas { get; } = new List<Reservas>();
}
