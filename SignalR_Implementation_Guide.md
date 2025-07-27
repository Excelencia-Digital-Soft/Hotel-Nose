# SignalR Implementation Guide

## Overview

The SignalR implementation has been improved with the following enhancements:
- Strongly-typed hub using `Hub<INotificationClient>`
- Authentication and authorization on hub endpoints
- Centralized notification service for reusability
- Better error handling and logging
- DTOs for structured message payloads

## Key Components

### 1. NotificationDto
```csharp
public class NotificationDto
{
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
}
```

### 2. INotificationClient Interface
```csharp
public interface INotificationClient
{
    Task ReceiveNotification(string type, string message, object? data = null);
    Task SubscriptionConfirmed(string message);
}
```

### 3. INotificationService Interface
```csharp
public interface INotificationService
{
    Task SendNotificationToAllAsync(string type, string message, object? data = null);
    Task SendNotificationToInstitutionAsync(int institucionId, string type, string message, object? data = null);
    Task SendNotificationToUserAsync(string userId, string type, string message, object? data = null);
    Task SendNotificationToGroupAsync(string groupName, string type, string message, object? data = null);
}
```

## Usage Examples

### In Controllers/Services

```csharp
public class MyController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public MyController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateReservation(ReservationDto dto)
    {
        // ... create reservation logic ...

        // Send notification to institution
        await _notificationService.SendNotificationToInstitutionAsync(
            institucionId: 1,
            type: "reservation_created",
            message: "Nueva reservación creada",
            data: new { reservationId = reservation.Id, roomName = reservation.RoomName }
        );

        return Ok();
    }
}
```

### Client-Side JavaScript Example

```javascript
// Create connection
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notifications", {
        accessTokenFactory: () => localStorage.getItem("token")
    })
    .withAutomaticReconnect()
    .build();

// Subscribe to notifications
connection.on("ReceiveNotification", (type, message, data) => {
    console.log(`Notification received: ${type} - ${message}`, data);
    
    // Handle different notification types
    switch(type) {
        case "warning":
            showWarning(message, data);
            break;
        case "error":
            showError(message, data);
            break;
        case "info":
            showInfo(message, data);
            break;
    }
});

// Subscribe to institution
async function subscribeToInstitution(institucionId) {
    try {
        await connection.invoke("SubscribeToInstitution", institucionId);
    } catch (err) {
        console.error("Error subscribing to institution:", err);
    }
}

// Start connection
async function start() {
    try {
        await connection.start();
        console.log("SignalR Connected");
        
        // Subscribe to institution after connection
        await subscribeToInstitution(1);
    } catch (err) {
        console.error("SignalR Connection Error:", err);
        setTimeout(start, 5000); // Retry after 5 seconds
    }
}

// Start the connection
start();
```

## Security Considerations

1. **Authentication Required**: The hub now requires authentication. Clients must provide a valid JWT token.
2. **Institution Isolation**: Notifications are scoped to institutions using groups.
3. **Role-Based Access**: Some notification endpoints (like broadcast) require admin roles.

## Migration from Old Implementation

### Before (Direct Hub Context)
```csharp
await _hubContext.Clients.All.SendAsync("ReceiveNotification", new { type = "info", message = "Hello" });
```

### After (Using Notification Service)
```csharp
await _notificationService.SendNotificationToAllAsync("info", "Hello", null);
```

## Testing the Implementation

Use the NotificationsController endpoints to test:

1. **Test notification to institution**:
   ```
   POST /api/v1/notifications/test
   {
       "type": "info",
       "message": "Test notification",
       "data": { "test": true }
   }
   ```

2. **Broadcast to all users** (requires Admin role):
   ```
   POST /api/v1/notifications/broadcast
   {
       "type": "announcement",
       "message": "System maintenance at 10 PM",
       "data": null
   }
   ```

3. **Send to specific user** (requires Admin role):
   ```
   POST /api/v1/notifications/user/{userId}
   {
       "type": "private",
       "message": "You have a new message",
       "data": { "messageId": 123 }
   }
   ```

## Best Practices

1. **Use appropriate notification types**: Define a consistent set of notification types (error, warning, info, success, etc.)
2. **Include relevant data**: Pass contextual data in the `data` field for client-side handling
3. **Handle disconnections**: Implement reconnection logic on the client side
4. **Log notifications**: The service logs all notifications for debugging
5. **Test thoroughly**: Test with multiple connected clients and different scenarios

## Troubleshooting

1. **401 Unauthorized**: Ensure the client is sending a valid JWT token
2. **No notifications received**: Check if the client is subscribed to the correct institution/group
3. **Connection drops**: Implement automatic reconnection on the client side