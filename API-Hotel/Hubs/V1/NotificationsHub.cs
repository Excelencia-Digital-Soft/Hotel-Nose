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
    private readonly IConnectionManager _connectionManager;

    public NotificationsHub(
        ILogger<NotificationsHub> logger,
        IConnectionManager connectionManager)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
    }

    #region Connection Management

    /// <summary>
    /// Handle client connection with automatic institution subscription and single connection enforcement
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        var connectionId = Context.ConnectionId;

        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning(
                "Client {ConnectionId} connected without user identification - disconnecting",
                connectionId
            );
            
            await Clients.Caller.ReceiveNotification(
                "error",
                "Authentication required",
                new { reason = "User identification missing" }
            );
            
            Context.Abort();
            return;
        }

        _logger.LogInformation(
            "Client connecting: ConnectionId={ConnectionId}, UserId={UserId}",
            connectionId,
            userId
        );

        // Get institution ID
        var institucionId = GetUserInstitucionId();
        
        // Add connection to manager (this will handle disconnecting any existing connection)
        var previousConnectionId = await _connectionManager.AddConnectionAsync(userId, connectionId, institucionId);
        
        if (!string.IsNullOrEmpty(previousConnectionId))
        {
            _logger.LogInformation(
                "Disconnecting previous connection {PreviousConnectionId} for user {UserId}",
                previousConnectionId,
                userId
            );
            
            // Notify the previous connection that it's being replaced
            await Clients.Client(previousConnectionId).ReceiveNotification(
                "info",
                "Connection replaced",
                new { reason = "New connection established from another location" }
            );
            
            // Force disconnect the previous connection
            await Clients.Client(previousConnectionId).ForceDisconnect(
                new { 
                    reason = "New connection established from another location", 
                    newConnectionId = connectionId,
                    timestamp = DateTime.UtcNow 
                }
            );
        }

        // Auto-subscribe to user's institution if available in claims
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
                $"Connected and subscribed to Institution {institucionId.Value} (Single connection enforced)"
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
    /// Handle client disconnection with proper cleanup
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;
        var userId = Context.UserIdentifier;

        // Remove connection from manager
        var removedUserId = await _connectionManager.RemoveConnectionAsync(connectionId);

        if (exception != null)
        {
            _logger.LogError(
                exception,
                "Client disconnected with error: ConnectionId={ConnectionId}, UserId={UserId}, RemovedUserId={RemovedUserId}",
                connectionId,
                userId,
                removedUserId
            );
        }
        else
        {
            _logger.LogInformation(
                "Client disconnected: ConnectionId={ConnectionId}, UserId={UserId}, RemovedUserId={RemovedUserId}",
                connectionId,
                userId,
                removedUserId
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

            // Get connection info from manager
            var managedConnection = userId != null ? await _connectionManager.GetConnectionInfoAsync(userId) : null;
            var totalConnections = await _connectionManager.GetActiveConnectionsCountAsync();

            var connectionInfo = new
            {
                connectionId = connectionId,
                userId = userId,
                institucionId = institucionId,
                timestamp = DateTime.UtcNow,
                isAuthenticated = Context.User?.Identity?.IsAuthenticated ?? false,
                totalActiveConnections = totalConnections,
                isManagedConnection = managedConnection != null,
                connectedAt = managedConnection?.ConnectedAt,
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

    /// <summary>
    /// Get connection statistics for administrators
    /// </summary>
    public async Task GetConnectionStats()
    {
        try
        {
            var totalConnections = await _connectionManager.GetActiveConnectionsCountAsync();
            var userId = Context.UserIdentifier;
            var isConnected = userId != null && await _connectionManager.IsUserConnectedAsync(userId);

            var stats = new
            {
                totalActiveConnections = totalConnections,
                currentUserId = userId,
                isUserConnected = isConnected,
                timestamp = DateTime.UtcNow
            };

            await Clients.Caller.ReceiveNotification(
                "info",
                "Connection statistics",
                stats
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting connection statistics for client {ConnectionId}",
                Context.ConnectionId
            );

            await Clients.Caller.ReceiveNotification(
                "error",
                "Failed to get connection statistics",
                new { error = ex.Message }
            );
        }
    }

    /// <summary>
    /// Join room-specific group for detailed updates
    /// </summary>
    /// <param name="roomId">Room ID to join</param>
    public async Task JoinRoomGroup(int roomId)
    {
        try
        {
            if (roomId <= 0)
            {
                await Clients.Caller.ReceiveNotification(
                    "error",
                    "Invalid Room ID",
                    new { error = "Room ID must be a positive number" }
                );
                return;
            }

            string groupName = $"Room_{roomId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            await Clients.Caller.ReceiveNotification(
                "info",
                $"Joined room group {roomId}",
                new { roomId = roomId, action = "joined" }
            );

            _logger.LogInformation(
                "User {UserId} joined room group {RoomId}",
                Context.UserIdentifier,
                roomId
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error joining room group {RoomId} for user {UserId}",
                roomId,
                Context.UserIdentifier
            );

            await Clients.Caller.ReceiveNotification(
                "error",
                "Failed to join room group",
                new { error = ex.Message }
            );
        }
    }

    /// <summary>
    /// Leave room-specific group
    /// </summary>
    /// <param name="roomId">Room ID to leave</param>
    public async Task LeaveRoomGroup(int roomId)
    {
        try
        {
            if (roomId <= 0)
            {
                await Clients.Caller.ReceiveNotification(
                    "error",
                    "Invalid Room ID",
                    new { error = "Room ID must be a positive number" }
                );
                return;
            }

            string groupName = $"Room_{roomId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            await Clients.Caller.ReceiveNotification(
                "info",
                $"Left room group {roomId}",
                new { roomId = roomId, action = "left" }
            );

            _logger.LogInformation(
                "User {UserId} left room group {RoomId}",
                Context.UserIdentifier,
                roomId
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error leaving room group {RoomId} for user {UserId}",
                roomId,
                Context.UserIdentifier
            );

            await Clients.Caller.ReceiveNotification(
                "error",
                "Failed to leave room group",
                new { error = ex.Message }
            );
        }
    }

    /// <summary>
    /// Subscribe to room progress updates
    /// </summary>
    /// <param name="roomId">Room ID to track progress</param>
    /// <param name="enable">Enable or disable progress tracking</param>
    public async Task SubscribeToRoomProgress(int roomId, bool enable)
    {
        try
        {
            if (roomId <= 0)
            {
                await Clients.Caller.ReceiveNotification(
                    "error",
                    "Invalid Room ID",
                    new { error = "Room ID must be a positive number" }
                );
                return;
            }

            string groupName = $"RoomProgress_{roomId}";
            
            if (enable)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                
                await Clients.Caller.ReceiveNotification(
                    "info",
                    $"Subscribed to progress updates for room {roomId}",
                    new { roomId = roomId, subscribed = true }
                );

                _logger.LogInformation(
                    "User {UserId} subscribed to progress updates for room {RoomId}",
                    Context.UserIdentifier,
                    roomId
                );
            }
            else
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
                
                await Clients.Caller.ReceiveNotification(
                    "info",
                    $"Unsubscribed from progress updates for room {roomId}",
                    new { roomId = roomId, subscribed = false }
                );

                _logger.LogInformation(
                    "User {UserId} unsubscribed from progress updates for room {RoomId}",
                    Context.UserIdentifier,
                    roomId
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error managing room progress subscription for room {RoomId} and user {UserId}",
                roomId,
                Context.UserIdentifier
            );

            await Clients.Caller.ReceiveNotification(
                "error",
                "Failed to manage room progress subscription",
                new { error = ex.Message }
            );
        }
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

