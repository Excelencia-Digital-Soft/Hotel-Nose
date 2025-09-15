using hotel.Interfaces;
using hotel.Models;

namespace hotel.Services;

/// <summary>
/// Service for handling reservation-specific notifications using SignalR
/// Follows Single Responsibility Principle and provides structured notification messages
/// </summary>
public class ReservationNotificationService : IReservationNotificationService
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<ReservationNotificationService> _logger;

    public ReservationNotificationService(
        INotificationService notificationService,
        ILogger<ReservationNotificationService> logger
    )
    {
        _notificationService =
            notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task NotifyReservationCreatedAsync(
        Reservas reserva,
        Habitaciones habitacion,
        Visitas visita,
        decimal totalAmount,
        string? promocionNombre = null,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var message = $"✅ Nueva reserva creada - Habitación: {habitacion.NombreHabitacion}";

            var notificationData = new
            {
                type = "reservation_created",
                reservaId = reserva.ReservaId,
                habitacionId = reserva.HabitacionId,
                habitacionNombre = habitacion.NombreHabitacion,
                visitaId = reserva.VisitaId,
                huespedIdentificador = visita.Identificador ?? "Sin identificación",
                huespedTelefono = visita.NumeroTelefono,
                huespedVehiculo = visita.PatenteVehiculo,
                totalHoras = reserva.TotalHoras ?? 0,
                totalMinutos = reserva.TotalMinutos ?? 0,
                totalAmount = totalAmount,
                fechaInicio = reserva.FechaReserva,
                promocionNombre = promocionNombre,
                esReserva = false,
                timestamp = DateTime.UtcNow,
            };

            await _notificationService.SendNotificationToInstitutionAsync(
                reserva.InstitucionID,
                "success",
                message,
                notificationData
            );

            _logger.LogInformation(
                "Sent reservation created notification for ReservaId={ReservaId}, HabitacionId={HabitacionId}",
                reserva.ReservaId,
                reserva.HabitacionId
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to send reservation created notification for ReservaId={ReservaId}",
                reserva.ReservaId
            );
        }
    }

    /// <inheritdoc/>
    public async Task NotifyReservationExtendedAsync(
        Reservas reserva,
        Habitaciones habitacion,
        int extensionHours,
        int extensionMinutes,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var message = $"⏱️ Reserva extendida - Habitación: {habitacion.NombreHabitacion}";

            var notificationData = new
            {
                type = "reservation_extended",
                reservaId = reserva.ReservaId,
                habitacionId = reserva.HabitacionId,
                habitacionNombre = habitacion.NombreHabitacion,
                totalHoras = reserva.TotalHoras ?? 0,
                totalMinutos = reserva.TotalMinutos ?? 0,
                extensionHoras = extensionHours,
                extensionMinutos = extensionMinutes,
                timestamp = DateTime.UtcNow,
            };

            await _notificationService.SendNotificationToInstitutionAsync(
                reserva.InstitucionID,
                "info",
                message,
                notificationData
            );

            _logger.LogInformation(
                "Sent reservation extended notification for ReservaId={ReservaId} by {Hours}h {Minutes}m",
                reserva.ReservaId,
                extensionHours,
                extensionMinutes
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to send reservation extended notification for ReservaId={ReservaId}",
                reserva.ReservaId
            );
        }
    }

    /// <inheritdoc/>
    public async Task NotifyReservationFinalizedAsync(
        int reservaId,
        Habitaciones habitacion,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var message = $"🏁 Reserva finalizada - Habitación: {habitacion.NombreHabitacion}";

            var notificationData = new
            {
                type = "reservation_finalized",
                reservaId = reservaId,
                habitacionId = habitacion.HabitacionId,
                habitacionNombre = habitacion.NombreHabitacion,
                timestamp = DateTime.UtcNow,
            };

            await _notificationService.SendNotificationToInstitutionAsync(
                institucionId,
                "success",
                message,
                notificationData
            );

            _logger.LogInformation(
                "Sent reservation finalized notification for ReservaId={ReservaId}, HabitacionId={HabitacionId}",
                reservaId,
                habitacion.HabitacionId
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to send reservation finalized notification for ReservaId={ReservaId}",
                reservaId
            );
        }
    }

    /// <inheritdoc/>
    public async Task NotifyReservationCancelledAsync(
        int reservaId,
        Habitaciones habitacion,
        int institucionId,
        string reason,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var message = $"❌ Reserva cancelada - Habitación: {habitacion.NombreHabitacion}";

            var notificationData = new
            {
                type = "reservation_cancelled",
                reservaId = reservaId,
                habitacionId = habitacion.HabitacionId,
                habitacionNombre = habitacion.NombreHabitacion,
                reason = reason,
                timestamp = DateTime.UtcNow,
            };

            await _notificationService.SendNotificationToInstitutionAsync(
                institucionId,
                "warning",
                message,
                notificationData
            );

            _logger.LogInformation(
                "Sent reservation cancelled notification for ReservaId={ReservaId}, Reason={Reason}",
                reservaId,
                reason
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to send reservation cancelled notification for ReservaId={ReservaId}",
                reservaId
            );
        }
    }

    /// <inheritdoc/>
    public async Task NotifyReservationPausedAsync(
        Reservas reserva,
        Habitaciones habitacion,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var message = $"⏸️ Reserva pausada - Habitación: {habitacion.NombreHabitacion}";

            var notificationData = new
            {
                type = "reservation_paused",
                reservaId = reserva.ReservaId,
                habitacionId = reserva.HabitacionId,
                habitacionNombre = habitacion.NombreHabitacion,
                pausaHoras = reserva.PausaHoras ?? 0,
                pausaMinutos = reserva.PausaMinutos ?? 0,
                timestamp = DateTime.UtcNow,
            };

            await _notificationService.SendNotificationToInstitutionAsync(
                reserva.InstitucionID,
                "info",
                message,
                notificationData
            );

            _logger.LogInformation(
                "Sent reservation paused notification for ReservaId={ReservaId}",
                reserva.ReservaId
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to send reservation paused notification for ReservaId={ReservaId}",
                reserva.ReservaId
            );
        }
    }

    /// <inheritdoc/>
    public async Task NotifyReservationResumedAsync(
        Reservas reserva,
        Habitaciones habitacion,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var message = $"▶️ Reserva reanudada - Habitación: {habitacion.NombreHabitacion}";

            var notificationData = new
            {
                type = "reservation_resumed",
                reservaId = reserva.ReservaId,
                habitacionId = reserva.HabitacionId,
                habitacionNombre = habitacion.NombreHabitacion,
                timestamp = DateTime.UtcNow,
            };

            await _notificationService.SendNotificationToInstitutionAsync(
                reserva.InstitucionID,
                "success",
                message,
                notificationData
            );

            _logger.LogInformation(
                "Sent reservation resumed notification for ReservaId={ReservaId}",
                reserva.ReservaId
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to send reservation resumed notification for ReservaId={ReservaId}",
                reserva.ReservaId
            );
        }
    }

    /// <inheritdoc/>
    public async Task NotifyReservationWarningAsync(
        int reservaId,
        string habitacionNombre,
        int institucionId,
        string warningMessage,
        object? additionalData = null,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var message = $"⚠️ {warningMessage} - Habitación: {habitacionNombre}";

            var notificationData = new
            {
                type = "reservation_warning",
                reservaId = reservaId,
                habitacionNombre = habitacionNombre,
                warningMessage = warningMessage,
                additionalData = additionalData,
                timestamp = DateTime.UtcNow,
            };

            await _notificationService.SendNotificationToInstitutionAsync(
                institucionId,
                "warning",
                message,
                notificationData
            );

            _logger.LogInformation(
                "Sent reservation warning notification for ReservaId={ReservaId}, Warning={Warning}",
                reservaId,
                warningMessage
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to send reservation warning notification for ReservaId={ReservaId}",
                reservaId
            );
        }
    }
}

