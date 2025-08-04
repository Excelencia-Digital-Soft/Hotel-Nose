using hotel.Interfaces;
using hotel.Hubs.V1;
using Microsoft.AspNetCore.SignalR;

namespace hotel.Services;

/// <summary>
/// SignalR-based notification service implementation
/// </summary>
public class SignalRNotificationService : INotificationService
{
    private readonly IHubContext<NotificationsHub, INotificationClient> _hubContext;
    private readonly ILogger<SignalRNotificationService> _logger;

    public SignalRNotificationService(
        IHubContext<NotificationsHub, INotificationClient> hubContext,
        ILogger<SignalRNotificationService> logger
    )
    {
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Send notification to all connected clients
    /// </summary>
    public async Task SendNotificationToAllAsync(string type, string message, object? data = null)
    {
        try
        {
            await _hubContext.Clients.All.ReceiveNotification(type, message, data);
            _logger.LogInformation(
                "Notification sent to all clients: Type={Type}, Message={Message}",
                type,
                message
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to all clients");
            throw;
        }
    }

    /// <summary>
    /// Send notification to all clients in a specific institution
    /// </summary>
    public async Task SendNotificationToInstitutionAsync(
        int institucionId,
        string type,
        string message,
        object? data = null
    )
    {
        try
        {
            var groupName = $"institution-{institucionId}";
            await _hubContext.Clients.Group(groupName).ReceiveNotification(type, message, data);
            _logger.LogInformation(
                "Notification sent to institution {InstitucionId}: Type={Type}, Message={Message}",
                institucionId,
                type,
                message
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error sending notification to institution {InstitucionId}",
                institucionId
            );
            throw;
        }
    }

    /// <summary>
    /// Send notification to a specific user
    /// </summary>
    public async Task SendNotificationToUserAsync(
        string userId,
        string type,
        string message,
        object? data = null
    )
    {
        try
        {
            await _hubContext.Clients.User(userId).ReceiveNotification(type, message, data);
            _logger.LogInformation(
                "Notification sent to user {UserId}: Type={Type}, Message={Message}",
                userId,
                type,
                message
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Send notification to a specific group
    /// </summary>
    public async Task SendNotificationToGroupAsync(
        string groupName,
        string type,
        string message,
        object? data = null
    )
    {
        try
        {
            await _hubContext.Clients.Group(groupName).ReceiveNotification(type, message, data);
            _logger.LogInformation(
                "Notification sent to group {GroupName}: Type={Type}, Message={Message}",
                groupName,
                type,
                message
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to group {GroupName}", groupName);
            throw;
        }
    }
}

