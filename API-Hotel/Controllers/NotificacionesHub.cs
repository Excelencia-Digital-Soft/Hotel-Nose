namespace hotel.NotificacionesHub;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class NotificationsHub : Hub
{
    public async Task SendNotification(string type, string message)
    {

        // Broadcast message to all connected clients
        await Clients.All.SendAsync("ReceiveNotification", new
        {
            type = type,
            message = message,
        });
    }
    public async Task SendNotificationInstitucion(string type, string message, int institucionID)
    {

        // Broadcast message to all connected clients
        await Clients.Group($"institution-{institucionID}").SendAsync("ReceiveNotification", new
        {
            type = type,
            message = message,
        });
    }
    public async Task SubscribeToInstitution(string institucionID)
    {
        try
        {
            if (string.IsNullOrEmpty(institucionID))
            {
                throw new ArgumentException("Invalid InstitucionID");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, $"institution-{institucionID}");
            await Clients.Caller.SendAsync("SubscriptionConfirmed", $"Subscribed to notifications for InstitucionID {institucionID}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SubscribeToInstitution: {ex.Message}");
            throw;
        }

    }
}