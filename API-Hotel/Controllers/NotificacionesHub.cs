namespace hotel.NotificacionesHub;
using hotel.DTOs.Common;
using hotel.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

[Authorize]
public class NotificationsHub : Hub<INotificationClient>
{
    private readonly ILogger<NotificationsHub> _logger;

    public NotificationsHub(ILogger<NotificationsHub> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SendNotification(NotificationDto notification)
    {
        try
        {
            // Broadcast message to all connected clients
            await Clients.All.ReceiveNotification(notification.Type, notification.Message, notification.Data);
            _logger.LogInformation("Notification sent to all clients: Type={Type}, Message={Message}", 
                notification.Type, notification.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to all clients");
            throw;
        }
    }

    public async Task SendNotificationInstitucion(NotificationDto notification, int institucionID)
    {
        try
        {
            // Broadcast message to all connected clients in the institution group
            await Clients.Group($"institution-{institucionID}")
                .ReceiveNotification(notification.Type, notification.Message, notification.Data);
            
            _logger.LogInformation("Notification sent to institution {InstitucionID}: Type={Type}, Message={Message}", 
                institucionID, notification.Type, notification.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to institution {InstitucionID}", institucionID);
            throw;
        }
    }

    public async Task SubscribeToInstitution(int institucionID)
    {
        try
        {
            if (institucionID <= 0)
            {
                await Clients.Caller.ReceiveNotification("error", "Invalid Institution ID", 
                    new { error = "Institution ID must be a positive number" });
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, $"institution-{institucionID}");
            await Clients.Caller.SubscriptionConfirmed($"Subscribed to notifications for Institution {institucionID}");
            
            _logger.LogInformation("Client {ConnectionId} subscribed to institution {InstitucionID}", 
                Context.ConnectionId, institucionID);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SubscribeToInstitution for client {ConnectionId}", Context.ConnectionId);
            await Clients.Caller.ReceiveNotification("error", "Failed to subscribe to institution", 
                new { error = ex.Message });
        }
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception != null)
        {
            _logger.LogError(exception, "Client disconnected with error: {ConnectionId}", Context.ConnectionId);
        }
        else
        {
            _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        }
        
        await base.OnDisconnectedAsync(exception);
    }
}