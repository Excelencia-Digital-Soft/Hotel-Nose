namespace hotel.Interfaces;

/// <summary>
/// Manages SignalR connections to ensure single connection per user
/// </summary>
public interface IConnectionManager
{
    /// <summary>
    /// Add a new connection, disconnecting any existing connection for the same user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="connectionId">Connection ID</param>
    /// <param name="institutionId">Institution ID for the user</param>
    /// <returns>Connection ID of any previous connection that was disconnected</returns>
    Task<string?> AddConnectionAsync(string userId, string connectionId, int? institutionId);

    /// <summary>
    /// Remove a connection when user disconnects
    /// </summary>
    /// <param name="connectionId">Connection ID to remove</param>
    /// <returns>User ID that was associated with the connection</returns>
    Task<string?> RemoveConnectionAsync(string connectionId);

    /// <summary>
    /// Get connection ID for a specific user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Connection ID or null if user is not connected</returns>
    Task<string?> GetConnectionIdAsync(string userId);

    /// <summary>
    /// Get user ID for a specific connection
    /// </summary>
    /// <param name="connectionId">Connection ID</param>
    /// <returns>User ID or null if connection doesn't exist</returns>
    Task<string?> GetUserIdAsync(string connectionId);

    /// <summary>
    /// Check if a user is currently connected
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>True if user has an active connection</returns>
    Task<bool> IsUserConnectedAsync(string userId);

    /// <summary>
    /// Get all active connections count
    /// </summary>
    /// <returns>Number of active connections</returns>
    Task<int> GetActiveConnectionsCountAsync();

    /// <summary>
    /// Get connection information for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Connection information or null if not connected</returns>
    Task<ConnectionInfo?> GetConnectionInfoAsync(string userId);
}

/// <summary>
/// Information about a user's connection
/// </summary>
public class ConnectionInfo
{
    public string UserId { get; set; } = string.Empty;
    public string ConnectionId { get; set; } = string.Empty;
    public int? InstitutionId { get; set; }
    public DateTime ConnectedAt { get; set; }
    public string? UserAgent { get; set; }
    public string? IpAddress { get; set; }
}