namespace ApiObjetos.NotificacionesHub;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class NotificationsHub : Hub
{
    public async Task SendNotification(string message)
    {
        // Broadcast message to all connected clients
        await Clients.All.SendAsync("ReceiveNotification", message);
    }
}