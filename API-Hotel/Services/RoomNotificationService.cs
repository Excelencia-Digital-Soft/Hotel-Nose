using hotel.DTOs.Rooms;
using hotel.Hubs.V1;
using hotel.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace hotel.Services;

public class RoomNotificationService : IRoomNotificationService
{
    private readonly IHubContext<NotificationsHub, INotificationClient> _hubContext;
    private readonly ILogger<RoomNotificationService> _logger;

    public RoomNotificationService(
        IHubContext<NotificationsHub, INotificationClient> hubContext,
        ILogger<RoomNotificationService> logger)
    {
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Notify room status change to all relevant clients
    /// </summary>
    public async Task NotifyRoomStatusChanged(int roomId, string status, int? visitaId, int institutionId, string? userId = null)
    {
        try
        {
            var payload = new
            {
                roomId = roomId,
                status = status, // "libre", "ocupada", "mantenimiento", "limpieza"
                visitaId = visitaId,
                timestamp = DateTime.UtcNow.ToString("O"),
                usuarioId = userId
            };

            // Send to institution group
            await _hubContext.Clients.Group($"institution-{institutionId}")
                .RoomStatusChanged(payload);

            // Send to room-specific group
            await _hubContext.Clients.Group($"Room_{roomId}")
                .RoomStatusChanged(payload);

            _logger.LogInformation(
                "Room {RoomId} status changed to {Status} for institution {InstitutionId} (VisitaId: {VisitaId})",
                roomId, status, institutionId, visitaId
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error notifying room status change for room {RoomId}",
                roomId
            );
            throw;
        }
    }

    /// <summary>
    /// Notify room progress update to subscribers
    /// </summary>
    public async Task NotifyRoomProgressUpdated(int roomId, int visitaId, RoomProgressDto progress, int institutionId)
    {
        try
        {
            var payload = new
            {
                roomId = roomId,
                visitaId = visitaId,
                startTime = progress.StartTime.ToString("O"),
                currentTime = DateTime.UtcNow.ToString("O"),
                progressPercentage = progress.ProgressPercentage,
                timeElapsed = progress.TimeElapsed,
                estimatedEndTime = progress.EstimatedEndTime?.ToString("O"),
                totalMinutes = progress.TotalMinutes,
                elapsedMinutes = progress.ElapsedMinutes
            };

            // Send only to room progress subscribers
            await _hubContext.Clients.Group($"RoomProgress_{roomId}")
                .RoomProgressUpdated(payload);

            _logger.LogDebug(
                "Room {RoomId} progress updated: {ProgressPercentage}% for visit {VisitaId}",
                roomId, progress.ProgressPercentage, visitaId
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error notifying room progress update for room {RoomId}",
                roomId
            );
            throw;
        }
    }

    /// <summary>
    /// Notify room reservation changes
    /// </summary>
    public async Task NotifyRoomReservationChanged(int roomId, int? reservaId, int? visitaId, string action, int institutionId)
    {
        try
        {
            var payload = new
            {
                roomId = roomId,
                reservaId = reservaId,
                visitaId = visitaId,
                action = action, // "created", "updated", "cancelled", "finalized"
                timestamp = DateTime.UtcNow.ToString("O")
            };

            // Send to institution group
            await _hubContext.Clients.Group($"institution-{institutionId}")
                .RoomReservationChanged(payload);

            // Send to room-specific group
            await _hubContext.Clients.Group($"Room_{roomId}")
                .RoomReservationChanged(payload);

            _logger.LogInformation(
                "Room {RoomId} reservation {Action} (ReservaId: {ReservaId}, VisitaId: {VisitaId}) for institution {InstitutionId}",
                roomId, action, reservaId, visitaId, institutionId
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error notifying room reservation change for room {RoomId}",
                roomId
            );
            throw;
        }
    }

    /// <summary>
    /// Notify room maintenance changes
    /// </summary>
    public async Task NotifyRoomMaintenanceChanged(int roomId, string maintenanceType, string status, string? description, int institutionId)
    {
        try
        {
            var payload = new
            {
                roomId = roomId,
                maintenanceType = maintenanceType, // "cleaning", "repair", "inspection"
                status = status, // "started", "in_progress", "completed", "cancelled"
                description = description,
                timestamp = DateTime.UtcNow.ToString("O")
            };

            // Send to institution group
            await _hubContext.Clients.Group($"institution-{institutionId}")
                .RoomMaintenanceChanged(payload);

            // Send to room-specific group
            await _hubContext.Clients.Group($"Room_{roomId}")
                .RoomMaintenanceChanged(payload);

            _logger.LogInformation(
                "Room {RoomId} maintenance {Status}: {MaintenanceType} for institution {InstitutionId}",
                roomId, status, maintenanceType, institutionId
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error notifying room maintenance change for room {RoomId}",
                roomId
            );
            throw;
        }
    }
}