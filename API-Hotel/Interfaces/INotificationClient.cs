namespace hotel.Interfaces;

public interface INotificationClient
{
    Task ReceiveNotification(string type, string message, object? data = null);
    Task SubscriptionConfirmed(string message);
    
    // Room-specific notifications
    Task RoomStatusChanged(object payload);
    Task RoomProgressUpdated(object payload);
    Task RoomReservationChanged(object payload);
    Task RoomMaintenanceChanged(object payload);
    
    // Connection management notifications
    Task ForceDisconnect(object payload);
}
