namespace hotel.Interfaces;

public interface INotificationService
{
    Task SendNotificationToAllAsync(string type, string message, object? data = null);
    Task SendNotificationToInstitutionAsync(int institucionId, string type, string message, object? data = null);
    Task SendNotificationToUserAsync(string userId, string type, string message, object? data = null);
    Task SendNotificationToGroupAsync(string groupName, string type, string message, object? data = null);
}
