using hotel.Data;
using hotel.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace hotel.Jobs;

/// <summary>
/// Cron job that monitors active reservations and sends notifications when they are about to expire
/// Runs every 5 minutes to check for reservations ending soon
/// </summary>
public class ReservationExpirationJob : CronBackgroundJob
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<ReservationExpirationJob> _logger;

    public ReservationExpirationJob(
        CronSettings<ReservationExpirationJob> settings,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<ReservationExpirationJob> logger
    )
        : base(settings.CronExpression, settings.TimeZone)
    {
        _serviceScopeFactory =
            serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task DoWork(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Starting reservation expiration check at {Timestamp}",
            DateTime.UtcNow
        );

        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
            var notificationService =
                scope.ServiceProvider.GetRequiredService<IReservationNotificationService>();

            // Get all active reservations (no end date and not cancelled)
            var activeReservations = await dbContext
                .Reservas.Include(r => r.Habitacion)
                .Include(r => r.Visita)
                .Where(r =>
                    r.FechaFin == null
                    && // Active reservation
                    r.FechaAnula == null
                    && // Not cancelled
                    r.FechaReserva != null
                    && // Has start date
                    r.TotalHoras != null
                    && // Has duration
                    r.Habitacion != null
                    && // Room exists
                    r.Visita != null
                ) // Visit exists
                .AsNoTracking()
                .ToListAsync(stoppingToken);

            if (!activeReservations.Any())
            {
                _logger.LogInformation("No active reservations found");
                return;
            }

            _logger.LogInformation(
                "Found {Count} active reservations to check",
                activeReservations.Count
            );

            var currentTime = DateTime.Now;
            var notificationsSent = 0;

            foreach (var reserva in activeReservations)
            {
                try
                {
                    // Calculate when the reservation should end
                    var startTime = reserva.FechaReserva!.Value;
                    var totalMinutes = (reserva.TotalHoras ?? 0) * 60 + (reserva.TotalMinutos ?? 0);

                    // Add paused time if any
                    var pausedMinutes =
                        (reserva.PausaHoras ?? 0) * 60 + (reserva.PausaMinutos ?? 0);
                    totalMinutes += pausedMinutes;

                    var endTime = startTime.AddMinutes(totalMinutes);
                    var timeRemaining = endTime - currentTime;

                    // Send different notifications based on time remaining
                    if (timeRemaining.TotalMinutes <= 0)
                    {
                        // Reservation has already expired
                        await notificationService.NotifyReservationWarningAsync(
                            reserva.ReservaId,
                            reserva.Habitacion!.NombreHabitacion!,
                            reserva.InstitucionID,
                            "⏰ Tiempo agotado - La reserva ha expirado",
                            new
                            {
                                overduenMinutes = Math.Abs(timeRemaining.TotalMinutes),
                                expectedEndTime = endTime,
                                actualTime = currentTime,
                            },
                            stoppingToken
                        );

                        notificationsSent++;
                        _logger.LogInformation(
                            "Sent expired notification for ReservaId={ReservaId}, Room={RoomName}, Overdue={OverdueMinutes}min",
                            reserva.ReservaId,
                            reserva.Habitacion.NombreHabitacion,
                            Math.Abs(timeRemaining.TotalMinutes)
                        );
                    }
                    else if (timeRemaining.TotalMinutes <= 5)
                    {
                        // 5 minutes or less remaining
                        await notificationService.NotifyReservationWarningAsync(
                            reserva.ReservaId,
                            reserva.Habitacion!.NombreHabitacion!,
                            reserva.InstitucionID,
                            $"🚨 Quedan {timeRemaining.TotalMinutes:F0} minutos",
                            new
                            {
                                remainingMinutes = timeRemaining.TotalMinutes,
                                endTime = endTime,
                                urgency = "critical",
                            },
                            stoppingToken
                        );

                        notificationsSent++;
                        _logger.LogInformation(
                            "Sent critical warning for ReservaId={ReservaId}, Room={RoomName}, Remaining={RemainingMinutes}min",
                            reserva.ReservaId,
                            reserva.Habitacion.NombreHabitacion,
                            timeRemaining.TotalMinutes
                        );
                    }
                    else if (timeRemaining.TotalMinutes <= 15)
                    {
                        // 15 minutes or less remaining
                        await notificationService.NotifyReservationWarningAsync(
                            reserva.ReservaId,
                            reserva.Habitacion!.NombreHabitacion!,
                            reserva.InstitucionID,
                            $"⚠️ Quedan {timeRemaining.TotalMinutes:F0} minutos",
                            new
                            {
                                remainingMinutes = timeRemaining.TotalMinutes,
                                endTime = endTime,
                                urgency = "medium",
                            },
                            stoppingToken
                        );

                        notificationsSent++;
                        _logger.LogInformation(
                            "Sent warning for ReservaId={ReservaId}, Room={RoomName}, Remaining={RemainingMinutes}min",
                            reserva.ReservaId,
                            reserva.Habitacion.NombreHabitacion,
                            timeRemaining.TotalMinutes
                        );
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error processing reservation {ReservaId} in expiration job",
                        reserva.ReservaId
                    );
                }
            }

            _logger.LogInformation(
                "Reservation expiration check completed. Processed {TotalReservations} reservations, sent {NotificationsSent} notifications",
                activeReservations.Count,
                notificationsSent
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in reservation expiration job");
        }
    }
}

