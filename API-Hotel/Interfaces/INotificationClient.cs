namespace hotel.Interfaces;

public interface INotificationClient
{
    Task ReceiveNotification(string type, string message, object? data = null);
    Task SubscriptionConfirmed(string message);
}
