using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiObjetos.Models;
using ApiObjetos.NotificacionesHub;
using ApiObjetos.Data;
public class ReservationMonitorService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IHubContext<NotificationsHub> _hubContext;

    public ReservationMonitorService(IServiceScopeFactory serviceScopeFactory, IHubContext<NotificationsHub> hubContext)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
            var habitacionesOcupadas = dbContext.Habitaciones
    .Where(h => h.VisitaID != null)
    .Select(h => h.VisitaID)
    .Distinct()
    .ToList();
            if (!habitacionesOcupadas.Any()) return; // No occupied rooms, exit early



            var reservasActivas = dbContext.Reservas
    .Where(r => habitacionesOcupadas.Contains(r.VisitaId))
    .ToList();

            foreach (var reserva in reservasActivas)
            {
                ScheduleNotification(reserva, stoppingToken);
            }
        }
    }

    public void ScheduleNotification(Reservas reserva, CancellationToken stoppingToken)
    {
        Task.Run(async () =>
        {
            DateTime endTime = reserva.FechaReserva.Value
                .AddHours(reserva.TotalHoras ?? 0)
                .AddMinutes(reserva.TotalMinutos ?? 0);

            DateTime warningTime = endTime.AddMinutes(-5); // 5-minute warning

            // Wait until 5 minutes before the reservation ends
            TimeSpan timeUntilWarning = warningTime - DateTime.Now;
            if (timeUntilWarning.TotalMilliseconds > 0)
            {
                await Task.Delay(timeUntilWarning, stoppingToken);
                await _hubContext.Clients.Group($"institution-{reserva.Visita.InstitucionID}").SendAsync("ReceiveNotification", new
                {
                    type = "warning",
                    roomId = reserva.HabitacionId,
                    message = $"⏳ La habitación {reserva.Habitacion.NombreHabitacion} le quedan 5 minutos!"
                }, stoppingToken);
            }

            // Wait until the reservation ends
            TimeSpan timeUntilEnd = endTime - DateTime.Now;
            if (timeUntilEnd.TotalMilliseconds > 0)
            {
                await Task.Delay(timeUntilEnd, stoppingToken);
                await _hubContext.Clients.Group($"institution-{reserva.Visita.InstitucionID}").SendAsync("ReceiveNotification", new
                {
                    type = "ended",
                    roomId = reserva.HabitacionId,
                    message = $"⚠️ La habitación {reserva.Habitacion.NombreHabitacion} se le acabó el tiempo!"
                }, stoppingToken);
            }
        }, stoppingToken);
    }
}

