using hotel.DTOs.Reservas;
using hotel.Models;

namespace hotel.Interfaces;

/// <summary>
/// Service for handling reservation-specific notifications
/// </summary>
public interface IReservationNotificationService
{
    /// <summary>
    /// Send notification when a new reservation is created
    /// </summary>
    Task NotifyReservationCreatedAsync(
        Reservas reserva, 
        Habitaciones habitacion, 
        Visitas visita, 
        decimal totalAmount, 
        string? promocionNombre = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send notification when a reservation is extended
    /// </summary>
    Task NotifyReservationExtendedAsync(
        Reservas reserva, 
        Habitaciones habitacion, 
        int extensionHours, 
        int extensionMinutes,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send notification when a reservation is finalized
    /// </summary>
    Task NotifyReservationFinalizedAsync(
        int reservaId, 
        Habitaciones habitacion, 
        int institucionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send notification when a reservation is cancelled
    /// </summary>
    Task NotifyReservationCancelledAsync(
        int reservaId, 
        Habitaciones habitacion, 
        int institucionId, 
        string reason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send notification when a reservation is paused
    /// </summary>
    Task NotifyReservationPausedAsync(
        Reservas reserva, 
        Habitaciones habitacion,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send notification when a reservation is resumed
    /// </summary>
    Task NotifyReservationResumedAsync(
        Reservas reserva, 
        Habitaciones habitacion,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send warning notification (e.g., time running out)
    /// </summary>
    Task NotifyReservationWarningAsync(
        int reservaId, 
        string habitacionNombre, 
        int institucionId, 
        string warningMessage,
        object? additionalData = null,
        CancellationToken cancellationToken = default);
}