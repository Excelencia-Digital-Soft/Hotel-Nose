using hotel.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace hotel.Hubs.V1;

/// <summary>
/// V1 SignalR Hub for real-time notifications
/// Follows clean architecture patterns with proper authentication and security
/// </summary>
[Authorize(AuthenticationSchemes = "Bearer")]
public class NotificationsHub : Hub<INotificationClient>
{
    private readonly ILogger<NotificationsHub> _logger;

    public NotificationsHub(ILogger<NotificationsHub> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Connection Management

    /// <summary>
    /// Handle client connection with automatic institution subscription
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        var connectionId = Context.ConnectionId;

        _logger.LogInformation(
            "Client connected: ConnectionId={ConnectionId}, UserId={UserId}",
            connectionId,
            userId
        );

        // Auto-subscribe to user's institution if available in claims
        var institucionId = GetUserInstitucionId();
        if (institucionId.HasValue)
        {
            await Groups.AddToGroupAsync(connectionId, $"institution-{institucionId.Value}");

            _logger.LogInformation(
                "Client {ConnectionId} auto-subscribed to institution {InstitucionId}",
                connectionId,
                institucionId.Value
            );

            // Confirm subscription to the client
            await Clients.Caller.SubscriptionConfirmed(
                $"Connected and subscribed to Institution {institucionId.Value}"
            );
        }
        else
        {
            _logger.LogWarning(
                "Client {ConnectionId} connected without institution information",
                connectionId
            );

            await Clients.Caller.ReceiveNotification(
                "warning",
                "Connected but institution information is missing",
                new { connectionId = connectionId }
            );
        }

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Handle client disconnection with proper logging
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;
        var userId = Context.UserIdentifier;

        if (exception != null)
        {
            _logger.LogError(
                exception,
                "Client disconnected with error: ConnectionId={ConnectionId}, UserId={UserId}",
                connectionId,
                userId
            );
        }
        else
        {
            _logger.LogInformation(
                "Client disconnected: ConnectionId={ConnectionId}, UserId={UserId}",
                connectionId,
                userId
            );
        }

        await base.OnDisconnectedAsync(exception);
    }

    #endregion

    #region Hub Methods

    /// <summary>
    /// Manual subscription to institution (with security validation)
    /// </summary>
    /// <param name="institucionId">Institution ID to subscribe to</param>
    public async Task SubscribeToInstitution(int institucionId)
    {
        try
        {
            if (institucionId <= 0)
            {
                await Clients.Caller.ReceiveNotification(
                    "error",
                    "Invalid Institution ID",
                    new { error = "Institution ID must be a positive number" }
                );
                return;
            }

            // Security validation: ensure user can only subscribe to their own institution
            var userInstitucionId = GetUserInstitucionId();
            if (!userInstitucionId.HasValue)
            {
                await Clients.Caller.ReceiveNotification(
                    "error",
                    "Institution not found in user claims",
                    new { error = "User institution information is missing" }
                );

                _logger.LogWarning(
                    "User {UserId} attempted to subscribe without institution claim",
                    Context.UserIdentifier
                );
                return;
            }

            if (userInstitucionId.Value != institucionId)
            {
                await Clients.Caller.ReceiveNotification(
                    "error",
                    "Unauthorized subscription attempt",
                    new { error = "You can only subscribe to your own institution" }
                );

                _logger.LogWarning(
                    "User {UserId} from institution {UserInstitution} attempted to subscribe to institution {RequestedInstitution}",
                    Context.UserIdentifier,
                    userInstitucionId.Value,
                    institucionId
                );
                return;
            }

            // Add to group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"institution-{institucionId}");
            await Clients.Caller.SubscriptionConfirmed(
                $"Successfully subscribed to notifications for Institution {institucionId}"
            );

            _logger.LogInformation(
                "Client {ConnectionId} (User: {UserId}) manually subscribed to institution {InstitucionId}",
                Context.ConnectionId,
                Context.UserIdentifier,
                institucionId
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error in SubscribeToInstitution for client {ConnectionId}",
                Context.ConnectionId
            );

            await Clients.Caller.ReceiveNotification(
                "error",
                "Failed to subscribe to institution",
                new { error = ex.Message }
            );
        }
    }

    /// <summary>
    /// Get connection information for debugging
    /// </summary>
    public async Task GetConnectionInfo()
    {
        try
        {
            var institucionId = GetUserInstitucionId();
            var userId = Context.UserIdentifier;
            var connectionId = Context.ConnectionId;

            var connectionInfo = new
            {
                connectionId = connectionId,
                userId = userId,
                institucionId = institucionId,
                timestamp = DateTime.UtcNow,
                isAuthenticated = Context.User?.Identity?.IsAuthenticated ?? false,
                claims = Context.User?.Claims?.Select(c => new { c.Type, c.Value }).ToList(),
            };

            await Clients.Caller.ReceiveNotification(
                "info",
                "Connection information",
                connectionInfo
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting connection info for client {ConnectionId}",
                Context.ConnectionId
            );

            await Clients.Caller.ReceiveNotification(
                "error",
                "Failed to get connection information",
                new { error = ex.Message }
            );
        }
    }

    /// <summary>
    /// Ping method for connection testing
    /// </summary>
    public async Task Ping()
    {
        await Clients.Caller.ReceiveNotification(
            "info",
            "Pong",
            new { timestamp = DateTime.UtcNow, connectionId = Context.ConnectionId }
        );
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Get current user's institution ID from JWT claims
    /// </summary>
    /// <returns>Institution ID or null if not found</returns>
    private int? GetUserInstitucionId()
    {
        var institucionIdClaim = Context.User?.FindFirst("InstitucionId")?.Value;
        if (
            !string.IsNullOrEmpty(institucionIdClaim)
            && int.TryParse(institucionIdClaim, out int institucionId)
        )
        {
            return institucionId;
        }

        return null;
    }

    #endregion
}

