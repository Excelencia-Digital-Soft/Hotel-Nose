using System.ComponentModel.DataAnnotations;
using hotel.DTOs.Caracteristicas;

namespace hotel.DTOs;

public class HabitacionDto
{
    public int HabitacionId { get; set; }
    public string NombreHabitacion { get; set; } = null!;
    public bool Disponible { get; set; }
    public int? VisitaId { get; set; }
    public int CategoriaHabitacionId { get; set; }
    public string? NombreCategoria { get; set; }
    public decimal? PrecioCategoria { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public int InstitucionId { get; set; }
    public string? Estado { get; set; }
    public string? Observaciones { get; set; }
    public ICollection<CaracteristicaDto> Caracteristicas { get; set; } = new List<CaracteristicaDto>();
    public ICollection<ImagenDto> Imagenes { get; set; } = new List<ImagenDto>();
}

public class HabitacionCreateDto
{
    [Required]
    [StringLength(100)]
    public string NombreHabitacion { get; set; } = null!;

    [Required]
    public int CategoriaHabitacionId { get; set; }

    [StringLength(50)]
    public string? Estado { get; set; }

    [StringLength(500)]
    public string? Observaciones { get; set; }

    public List<int>? CaracteristicaIds { get; set; }
}

public class HabitacionUpdateDto
{
    [StringLength(100)]
    public string? NombreHabitacion { get; set; }

    public int? CategoriaHabitacionId { get; set; }

    [StringLength(50)]
    public string? Estado { get; set; }

    [StringLength(500)]
    public string? Observaciones { get; set; }

    public bool? Activo { get; set; }

    public List<int>? CaracteristicaIds { get; set; }
}

public class HabitacionStatsDto
{
    public int TotalHabitaciones { get; set; }
    public int HabitacionesDisponibles { get; set; }
    public int HabitacionesOcupadas { get; set; }
    public int HabitacionesInactivas { get; set; }
    public decimal PorcentajeOcupacion { get; set; }
    public Dictionary<string, int> OcupacionPorCategoria { get; set; } = new();
    public DateTime FechaConsulta { get; set; } = DateTime.UtcNow;
}

public class ImagenDto
{
    public int ImagenId { get; set; }
    public string NombreArchivo { get; set; } = null!;
    public string? UrlCompleta { get; set; }
}

public class HabitacionAvailabilityDto
{
    [Required]
    public bool Disponible { get; set; }
    
    public string? Observaciones { get; set; }
}

public class HabitacionCompleteDto
{
    public int HabitacionId { get; set; }
    public string NombreHabitacion { get; set; } = null!;
    public int? CategoriaId { get; set; }
    public bool? Disponible { get; set; }
    public DateTime? ProximaReserva { get; set; }
    public int? UsuarioId { get; set; }
    public DateTime? FechaRegistro { get; set; }
    public bool? Anulado { get; set; }
    public int? VisitaID { get; set; }
    public decimal? Precio { get; set; }
    public bool PedidosPendientes { get; set; }
    public ReservaActivaDto? ReservaActiva { get; set; }
    public ReservaActivaDto? Reserva { get; set; }
    public VisitaBasicDto? Visita { get; set; }
    public ICollection<int> Imagenes { get; set; } = new List<int>();
    public ICollection<CaracteristicaDto> Caracteristicas { get; set; } = new List<CaracteristicaDto>();
}


public class ReservaActivaDto
{
    public int ReservaId { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public decimal? MontoTotal { get; set; }
    public int? TotalHoras { get; set; }
    public int? TotalMinutos { get; set; }
    public int? MovimientoId { get; set; }
    public int? PromocionId { get; set; }
    public int? PausaHoras { get; set; }
    public int? PausaMinutos { get; set; }
    public DateTime? FechaRegistro { get; set; }
}

public class VisitaBasicDto
{
    public int VisitaId { get; set; }
    public DateTime? FechaIngreso { get; set; }
    public DateTime? FechaSalida { get; set; }
    public int? ClienteId { get; set; }
    public string? NombreCompleto { get; set; }
    public string? PatenteVehiculo { get; set; }
    public string? NumeroTelefono { get; set; }
    public DateTime? FechaRegistro { get; set; }
}