using System.Collections.Concurrent;
using hotel.Data;
using hotel.Interfaces;
using hotel.Models;
using hotel.NotificacionesHub;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

public class ReservationMonitorService : BackgroundService
{
    private readonly ConcurrentDictionary<int, CancellationTokenSource> _activeTimers = new();
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly INotificationService _notificationService;

    public ReservationMonitorService(
        IServiceScopeFactory serviceScopeFactory,
        INotificationService notificationService
    )
    {
        _serviceScopeFactory = serviceScopeFactory;
        _notificationService = notificationService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
            var habitacionesOcupadas = await dbContext
                .Habitaciones.Where(h => h.VisitaID != null)
                .Select(h => h.VisitaID)
                .Distinct()
                .ToListAsync();
            if (!habitacionesOcupadas.Any())
                return; // No occupied rooms, exit early

            var reservasActivas = dbContext
                .Reservas.Where(r => habitacionesOcupadas.Contains(r.VisitaId))
                .ToList();

            foreach (var reserva in reservasActivas)
            {
                ScheduleNotification(reserva);
            }
        }
    }

    public void CancelNotification(int reservaId)
    {
        if (_activeTimers.TryRemove(reservaId, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
        }
    }

    public void ScheduleNotification(Reservas reserva)
    {
        // Cancel previous timer if one exists
        if (_activeTimers.TryGetValue(reserva.ReservaId, out var existingCts))
        {
            existingCts.Cancel();
            existingCts.Dispose();
        }

        var cts = new CancellationTokenSource();
        _activeTimers[reserva.ReservaId] = cts;

        Task.Run(
            async () =>
            {
                try
                {
                    DateTime endTime = reserva
                        .FechaReserva!.Value.AddHours(reserva.TotalHoras ?? 0)
                        .AddMinutes(reserva.TotalMinutos ?? 0);

                    DateTime warningTime = endTime.AddMinutes(-5);

                    TimeSpan timeUntilWarning = warningTime - DateTime.Now;
                    if (timeUntilWarning.TotalMilliseconds > 0)
                    {
                        await Task.Delay(timeUntilWarning, cts.Token);
                        await _notificationService.SendNotificationToInstitutionAsync(
                            reserva.Visita!.InstitucionID,
                            "warning",
                            $"⏳ La habitación {reserva.Habitacion!.NombreHabitacion} le quedan 5 minutos!",
                            new { roomId = reserva.HabitacionId }
                        );
                    }

                    TimeSpan timeUntilEnd = endTime - DateTime.Now;
                    if (timeUntilEnd.TotalMilliseconds > 0)
                    {
                        await Task.Delay(timeUntilEnd, cts.Token);
                        await _notificationService.SendNotificationToInstitutionAsync(
                            reserva.Visita!.InstitucionID,
                            "ended",
                            $"⚠️ La habitación {reserva.Habitacion!.NombreHabitacion} se le acabó el tiempo!",
                            new { roomId = reserva.HabitacionId }
                        );
                    }
                }
                catch (TaskCanceledException)
                {
                    // Silently handle canceled task
                }
                finally
                {
                    _activeTimers.TryRemove(reserva.ReservaId, out _);
                    cts.Dispose();
                }
            },
            cts.Token
        );
    }
}
