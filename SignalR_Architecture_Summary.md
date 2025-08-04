# SignalR Architecture Summary - Hotel Management System

## 🏗️ Current Implementation Status

### ✅ What's Working Well

1. **Strongly-Typed Hub**: `NotificationsHub` implements `Hub<INotificationClient>` for type safety
2. **Authentication Required**: Hub is protected with `[Authorize]` attribute
3. **Service Pattern**: `SignalRNotificationService` implements `INotificationService` for decoupled architecture
4. **Institution Isolation**: Uses SignalR groups (`institution-{id}`) to isolate notifications by institution
5. **Auto-Subscription**: Users are automatically subscribed to their institution's group on connection
6. **Structured Messages**: Uses `NotificationDto` for consistent message format

### 🔄 Changes Made

1. **Removed Background Service**: 
   - Deleted `ReservationMonitorService` (was in `/Controllers/MonitorService.cs`)
   - Updated `ReservasController` to use `INotificationService` directly
   - Notifications are now sent in real-time when events occur

2. **Enhanced Security**:
   - Added validation in `SubscribeToInstitution` to ensure users can only subscribe to their own institution
   - Auto-subscription based on user claims on connection
   - Improved logging for security events

3. **Simplified Architecture**:
   - Single unified implementation of SignalR
   - No redundant services or background processes
   - Clear separation of concerns

## 📡 Architecture Overview

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│   Frontend      │────▶│ NotificationsHub│────▶│ SignalR Groups  │
│   (SignalR.js)  │     │   (WebSocket)   │     │ institution-{id}│
└─────────────────┘     └─────────────────┘     └─────────────────┘
                                │
                                ▼
┌─────────────────┐     ┌─────────────────┐
│ Business Logic  │────▶│INotificationSvc │
│  (Services)     │     │ (Send to groups)│
└─────────────────┘     └─────────────────┘
```

## 🔒 Security Features

1. **Authentication**: Bearer token required for hub connection
2. **Institution Isolation**: Users only receive notifications for their institution
3. **Claim Validation**: Server validates user's institution claim before allowing subscriptions
4. **Auto-Subscription**: Based on JWT claims, no manual institution ID needed
5. **Audit Logging**: All subscription attempts are logged

## 🚀 Usage Examples

### Backend - Sending Notifications

```csharp
// Inject INotificationService in your service/controller
private readonly INotificationService _notificationService;

// Send to specific institution
await _notificationService.SendNotificationToInstitutionAsync(
    institucionId: 1,
    type: "info",
    message: "New reservation created",
    data: new { reservationId = 123 }
);

// Send to specific user
await _notificationService.SendNotificationToUserAsync(
    userId: "user-guid",
    type: "warning",
    message: "Payment due soon",
    data: new { amount = 100.50 }
);
```

### Frontend - Receiving Notifications

```javascript
// Initialize connection with auth token
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notifications", {
        accessTokenFactory: () => localStorage.getItem("token")
    })
    .withAutomaticReconnect()
    .build();

// Handle notifications
connection.on("ReceiveNotification", (type, message, data) => {
    console.log(`${type}: ${message}`, data);
    // Update UI accordingly
});

// Start connection
await connection.start();
// Note: Server auto-subscribes to user's institution
```

## 📋 Notification Types

- `info` - General information
- `warning` - Warnings (e.g., reservation ending soon)
- `error` - Error notifications
- `success` - Success confirmations
- Custom types as needed

## 🎯 Best Practices

1. **Always use INotificationService** - Don't inject IHubContext directly
2. **Include contextual data** - Pass relevant IDs and details in the data parameter
3. **Use appropriate notification types** - Helps frontend handle different scenarios
4. **Test multi-tenancy** - Ensure notifications don't leak across institutions
5. **Handle disconnections** - Frontend should implement automatic reconnection

## 🔧 Configuration

The SignalR hub is mapped at `/notifications` endpoint in `MiddlewareExtensions.cs`:

```csharp
app.MapHub<NotificationsHub>("/notifications")
    .RequireAuthorization();
```

## 📝 Future Considerations

1. **Message Persistence**: Consider storing critical notifications for offline users
2. **Delivery Confirmation**: Implement acknowledgment system for critical notifications
3. **Rate Limiting**: Add throttling to prevent notification spam
4. **Analytics**: Track notification delivery and engagement metrics

## ⚡ Performance Notes

- SignalR uses WebSockets when available, falling back to other transports
- Groups are efficient for broadcasting to subsets of users
- The service is registered as Singleton for performance
- No background services running continuously