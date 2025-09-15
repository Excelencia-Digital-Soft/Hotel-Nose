using hotel.Interfaces;
using System.Collections.Concurrent;

namespace hotel.Services;

/// <summary>
/// Thread-safe connection manager for SignalR connections
/// Ensures only one connection per user at a time
/// </summary>
public class ConnectionManager : IConnectionManager
{
    private readonly ConcurrentDictionary<string, hotel.Interfaces.ConnectionInfo> _userConnections = new();
    private readonly ConcurrentDictionary<string, string> _connectionToUser = new();
    private readonly ILogger<ConnectionManager> _logger;

    public ConnectionManager(ILogger<ConnectionManager> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<string?> AddConnectionAsync(string userId, string connectionId, int? institutionId)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(connectionId))
        {
            throw new ArgumentException("UserId and ConnectionId cannot be null or empty");
        }

        string? previousConnectionId = null;

        // Check if user already has a connection
        if (_userConnections.TryGetValue(userId, out var existingConnection))
        {
            previousConnectionId = existingConnection.ConnectionId;
            
            // Remove the old connection mappings
            _connectionToUser.TryRemove(existingConnection.ConnectionId, out _);
            
            _logger.LogInformation(
                "User {UserId} had existing connection {OldConnectionId}, will be replaced with {NewConnectionId}",
                userId, existingConnection.ConnectionId, connectionId
            );
        }

        // Create new connection info
        var connectionInfo = new hotel.Interfaces.ConnectionInfo
        {
            UserId = userId,
            ConnectionId = connectionId,
            InstitutionId = institutionId,
            ConnectedAt = DateTime.UtcNow
        };

        // Add/Update the connection mappings
        _userConnections.AddOrUpdate(userId, connectionInfo, (key, oldValue) => connectionInfo);
        _connectionToUser.AddOrUpdate(connectionId, userId, (key, oldValue) => userId);

        _logger.LogInformation(
            "Added connection {ConnectionId} for user {UserId} (Institution: {InstitutionId})",
            connectionId, userId, institutionId
        );

        return await Task.FromResult(previousConnectionId);
    }

    /// <inheritdoc />
    public async Task<string?> RemoveConnectionAsync(string connectionId)
    {
        if (string.IsNullOrEmpty(connectionId))
        {
            return await Task.FromResult<string?>(null);
        }

        string? userId = null;

        if (_connectionToUser.TryRemove(connectionId, out userId))
        {
            if (!string.IsNullOrEmpty(userId))
            {
                _userConnections.TryRemove(userId, out _);
                
                _logger.LogInformation(
                    "Removed connection {ConnectionId} for user {UserId}",
                    connectionId, userId
                );
            }
        }

        return await Task.FromResult(userId);
    }

    /// <inheritdoc />
    public async Task<string?> GetConnectionIdAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return await Task.FromResult<string?>(null);
        }

        if (_userConnections.TryGetValue(userId, out var connectionInfo))
        {
            return await Task.FromResult(connectionInfo.ConnectionId);
        }

        return await Task.FromResult<string?>(null);
    }

    /// <inheritdoc />
    public async Task<string?> GetUserIdAsync(string connectionId)
    {
        if (string.IsNullOrEmpty(connectionId))
        {
            return await Task.FromResult<string?>(null);
        }

        if (_connectionToUser.TryGetValue(connectionId, out var userId))
        {
            return await Task.FromResult(userId);
        }

        return await Task.FromResult<string?>(null);
    }

    /// <inheritdoc />
    public async Task<bool> IsUserConnectedAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return await Task.FromResult(false);
        }

        return await Task.FromResult(_userConnections.ContainsKey(userId));
    }

    /// <inheritdoc />
    public async Task<int> GetActiveConnectionsCountAsync()
    {
        return await Task.FromResult(_userConnections.Count);
    }

    /// <inheritdoc />
    public async Task<hotel.Interfaces.ConnectionInfo?> GetConnectionInfoAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return await Task.FromResult<hotel.Interfaces.ConnectionInfo?>(null);
        }

        if (_userConnections.TryGetValue(userId, out var connectionInfo))
        {
            return await Task.FromResult(connectionInfo);
        }

        return await Task.FromResult<hotel.Interfaces.ConnectionInfo?>(null);
    }
}