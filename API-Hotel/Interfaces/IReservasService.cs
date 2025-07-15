using hotel.DTOs.Common;
using hotel.DTOs.Reservas;

namespace hotel.Interfaces;

/// <summary>
/// Service interface for managing reservas (reservations)
/// </summary>
public interface IReservasService
{
    /// <summary>
    /// Finalize a reservation and free the room
    /// </summary>
    /// <param name="habitacionId">Room ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response</returns>
    Task<ApiResponse> FinalizeReservationAsync(
        int habitacionId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Pause an active occupation
    /// </summary>
    /// <param name="visitaId">Visit ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response</returns>
    Task<ApiResponse> PauseOccupationAsync(
        int visitaId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resume a paused occupation
    /// </summary>
    /// <param name="visitaId">Visit ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response</returns>
    Task<ApiResponse> ResumeOccupationAsync(
        int visitaId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update reservation promotion
    /// </summary>
    /// <param name="reservaId">Reservation ID</param>
    /// <param name="promocionId">Promotion ID (null to remove)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated reservation</returns>
    Task<ApiResponse<ReservaDto>> UpdateReservationPromotionAsync(
        int reservaId, 
        int? promocionId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancel/void a reservation
    /// </summary>
    /// <param name="reservaId">Reservation ID</param>
    /// <param name="reason">Cancellation reason</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response</returns>
    Task<ApiResponse> CancelReservationAsync(
        int reservaId, 
        string reason, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Extend reservation time
    /// </summary>
    /// <param name="reservaId">Reservation ID</param>
    /// <param name="additionalHours">Additional hours</param>
    /// <param name="additionalMinutes">Additional minutes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated reservation</returns>
    Task<ApiResponse<ReservaDto>> ExtendReservationAsync(
        int reservaId, 
        int additionalHours, 
        int additionalMinutes, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get reservation by ID
    /// </summary>
    /// <param name="reservaId">Reservation ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Reservation details</returns>
    Task<ApiResponse<ReservaDto>> GetReservationByIdAsync(
        int reservaId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get active reservations for an institution
    /// </summary>
    /// <param name="institucionId">Institution ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of active reservations</returns>
    Task<ApiResponse<IEnumerable<ReservaDto>>> GetActiveReservationsAsync(
        int institucionId, 
        CancellationToken cancellationToken = default);
}