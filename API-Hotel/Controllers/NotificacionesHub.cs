namespace hotel.NotificacionesHub;
using hotel.DTOs.Common;
using hotel.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading.Tasks;

[Authorize]
[Obsolete("This hub is deprecated. Use hotel.Hubs.V1.NotificationsHub instead.")]
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

            // Security check: Verify user belongs to the institution they're trying to subscribe to
            var userInstitucionIdClaim = Context.User?.FindFirst("InstitucionId")?.Value;
            if (string.IsNullOrEmpty(userInstitucionIdClaim) || 
                !int.TryParse(userInstitucionIdClaim, out int userInstitucionId))
            {
                await Clients.Caller.ReceiveNotification("error", "Institution not found in user claims", 
                    new { error = "User institution information is missing" });
                _logger.LogWarning("User {UserId} attempted to subscribe without institution claim", 
                    Context.UserIdentifier);
                return;
            }

            // Ensure user can only subscribe to their own institution
            if (userInstitucionId != institucionID)
            {
                await Clients.Caller.ReceiveNotification("error", "Unauthorized subscription attempt", 
                    new { error = "You can only subscribe to your own institution" });
                _logger.LogWarning("User {UserId} from institution {UserInstitution} attempted to subscribe to institution {RequestedInstitution}", 
                    Context.UserIdentifier, userInstitucionId, institucionID);
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, $"institution-{institucionID}");
            await Clients.Caller.SubscriptionConfirmed($"Subscribed to notifications for Institution {institucionID}");
            
            _logger.LogInformation("Client {ConnectionId} (User: {UserId}) subscribed to institution {InstitucionID}", 
                Context.ConnectionId, Context.UserIdentifier, institucionID);
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
        _logger.LogInformation("Client connected: {ConnectionId} (User: {UserId})", 
            Context.ConnectionId, Context.UserIdentifier);
        
        // Auto-subscribe to user's institution if available in claims
        var institucionIdClaim = Context.User?.FindFirst("InstitucionId")?.Value;
        if (!string.IsNullOrEmpty(institucionIdClaim) && int.TryParse(institucionIdClaim, out int institucionId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"institution-{institucionId}");
            _logger.LogInformation("Client {ConnectionId} auto-subscribed to institution {InstitucionId}", 
                Context.ConnectionId, institucionId);
        }
        
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