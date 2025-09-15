using System.ComponentModel.DataAnnotations;

namespace hotel.DTOs;

/// <summary>
/// Optimized DTO for habitaciones with minimal data for better performance
/// Use this for general habitacion listings where detailed information is not needed
/// </summary>
public class HabitacionOptimizedDto
{
    /// <summary>
    /// Critical fields for all habitaciones
    /// </summary>
    public int HabitacionId { get; set; }
    public string NombreHabitacion { get; set; } = null!;
    public bool? Disponible { get; set; }
    public decimal? Precio { get; set; }
    public int? CategoriaId { get; set; }

    /// <summary>
    /// Only for occupied habitaciones
    /// </summary>
    public bool PedidosPendientes { get; set; }
    public int? VisitaID { get; set; }

    /// <summary>
    /// Nested objects optimized (only when habitacion is occupied)
    /// </summary>
    public ReservaOptimizedDto? ReservaActiva { get; set; }
    public VisitaOptimizedDto? Visita { get; set; }
}

/// <summary>
/// Minimal DTO for habitaciones libres - contains only essential data
/// Estimated 70% data reduction compared to HabitacionCompleteDto
/// </summary>
public class HabitacionLibreDto
{
    public int HabitacionId { get; set; }
    public string NombreHabitacion { get; set; } = null!;
    public bool? Disponible { get; set; }
    public decimal? Precio { get; set; }
    public int? CategoriaId { get; set; }
}

/// <summary>
/// Optimized DTO for reserva data with only used fields
/// Removed: MontoTotal, MovimientoId, FechaRegistro
/// </summary>
public class ReservaOptimizedDto
{
    public int ReservaId { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public int? TotalHoras { get; set; }
    public int? TotalMinutos { get; set; }
    
    /// <summary>
    /// Optional fields - only include when they have values
    /// </summary>
    public int? PausaHoras { get; set; }
    public int? PausaMinutos { get; set; }
    public int? PromocionId { get; set; }
}

/// <summary>
/// Optimized DTO for visita data with only used fields
/// Removed: FechaIngreso, FechaSalida, ClienteId, FechaRegistro
/// Note: Removed reservaActiva field to avoid duplication (it's in the main object)
/// </summary>
public class VisitaOptimizedDto
{
    public int VisitaId { get; set; }
    public string? NombreCompleto { get; set; }
    
    /// <summary>
    /// Optional fields - only include when they have values
    /// </summary>
    public string? NumeroTelefono { get; set; }
    public string? PatenteVehiculo { get; set; }
}

/// <summary>
/// DTO for statistics and bulk operations
/// </summary>
public class HabitacionBulkStatsDto
{
    public IEnumerable<HabitacionLibreDto> HabitacionesLibres { get; set; } = new List<HabitacionLibreDto>();
    public IEnumerable<HabitacionOptimizedDto> HabitacionesOcupadas { get; set; } = new List<HabitacionOptimizedDto>();
    public HabitacionStatsDto Stats { get; set; } = new();
}

/// <summary>
/// Request DTO for filtering habitaciones
/// </summary>
public class HabitacionFilterDto
{
    public bool? IncludeInactive { get; set; } = false;
    public bool? OnlyAvailable { get; set; }
    public bool? OnlyOccupied { get; set; }
    public int? CategoriaId { get; set; }
    public bool? WithPendingOrders { get; set; }
}