using System.ComponentModel.DataAnnotations;

namespace hotel.DTOs.Reservas;

/// <summary>
/// DTO for reserva (reservation) information
/// </summary>
public class ReservaDto
{
    public int ReservaId { get; set; }
    public int HabitacionId { get; set; }
    public int VisitaId { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public int TotalHoras { get; set; }
    public int TotalMinutos { get; set; }
    public int? PromocionId { get; set; }
    public string? PromocionNombre { get; set; }
    public decimal? PromocionTarifa { get; set; }
    public int PausaHoras { get; set; }
    public int PausaMinutos { get; set; }
    public bool EsReserva { get; set; }
    public bool Activo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating a new reservation
/// </summary>
public class ReservaCreateDto
{
    /// <summary>
    /// Room ID to reserve
    /// </summary>
    [Required(ErrorMessage = "Room ID is required")]
    public int HabitacionId { get; set; }

    /// <summary>
    /// Optional promotion ID to apply
    /// </summary>
    public int? PromocionId { get; set; }

    /// <summary>
    /// Reservation start date and time
    /// </summary>
    [Required(ErrorMessage = "Reservation start date is required")]
    public DateTime FechaInicio { get; set; }

    /// <summary>
    /// Reservation end date and time (optional - if not provided, calculated from duration)
    /// </summary>
    public DateTime? FechaFin { get; set; }

    /// <summary>
    /// Total hours for the reservation
    /// </summary>
    [Range(0, 168, ErrorMessage = "Hours must be between 0 and 168 (1 week)")]
    public int TotalHoras { get; set; }

    /// <summary>
    /// Total minutes for the reservation (additional to hours)
    /// </summary>
    [Range(0, 59, ErrorMessage = "Minutes must be between 0 and 59")]
    public int TotalMinutos { get; set; }

    /// <summary>
    /// Indicates if this is a reservation (true) or walk-in (false)
    /// </summary>
    public bool EsReserva { get; set; } = true;

    /// <summary>
    /// Guest information
    /// </summary>
    [Required]
    public GuestInfoDto Guest { get; set; } = new();
}

/// <summary>
/// DTO for guest information
/// </summary>
public class GuestInfoDto
{
    /// <summary>
    /// Vehicle license plate (optional)
    /// </summary>
    [StringLength(20, ErrorMessage = "License plate cannot exceed 20 characters")]
    public string? PatenteVehiculo { get; set; }

    /// <summary>
    /// Guest phone number (optional)
    /// </summary>
    [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
    [Phone(ErrorMessage = "Invalid phone number format")]
    public string? NumeroTelefono { get; set; }

    /// <summary>
    /// Guest identifier (document number, name, etc.)
    /// </summary>
    [StringLength(100, ErrorMessage = "Identifier cannot exceed 100 characters")]
    public string? Identificador { get; set; }
}

/// <summary>
/// DTO for updating reservation promotion
/// </summary>
public class ReservaPromocionUpdateDto
{
    public int? PromocionId { get; set; }
}

/// <summary>
/// DTO for extending reservation time
/// </summary>
public class ReservaExtensionDto
{
    [Required]
    [Range(0, 24, ErrorMessage = "Additional hours must be between 0 and 24")]
    public int AdditionalHours { get; set; }
    
    [Range(0, 59, ErrorMessage = "Additional minutes must be between 0 and 59")]
    public int AdditionalMinutes { get; set; }
}

/// <summary>
/// DTO for reservation cancellation
/// </summary>
public class ReservaCancelDto
{
    [Required]
    [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// DTO for occupation pause/resume
/// </summary>
public class OccupationControlDto
{
    public string? Reason { get; set; }
    public DateTime ActionTime { get; set; }
}

/// <summary>
/// DTO for reservation summary
/// </summary>
public class ReservaSummaryDto
{
    public int ReservaId { get; set; }
    public string HabitacionNombre { get; set; } = string.Empty;
    public string VisitaNombre { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public int TotalMinutos { get; set; }
    public int MinutosRestantes { get; set; }
    public int MinutosExtra { get; set; }
    public bool IsPaused { get; set; }
    public bool IsOvertime { get; set; }
    public decimal CostoBase { get; set; }
    public decimal CostoExtra { get; set; }
    public decimal CostoTotal { get; set; }
}