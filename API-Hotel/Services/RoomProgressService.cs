using hotel.DTOs.Rooms;
using hotel.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace hotel.Services;

/// <summary>
/// Background service that sends room progress updates every minute
/// </summary>
public class RoomProgressService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RoomProgressService> _logger;
    private readonly TimeSpan _updateInterval = TimeSpan.FromMinutes(1);

    public RoomProgressService(
        IServiceProvider serviceProvider,
        ILogger<RoomProgressService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Room Progress Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await UpdateAllRoomProgress(stoppingToken);
                await Task.Delay(_updateInterval, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // Expected when cancellation is requested
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Room Progress Service. Retrying in 30 seconds...");
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }

        _logger.LogInformation("Room Progress Service stopped");
    }

    private async Task UpdateAllRoomProgress(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var roomNotificationService = scope.ServiceProvider.GetRequiredService<IRoomNotificationService>();
        var habitacionesService = scope.ServiceProvider.GetRequiredService<IHabitacionesService>();

        try
        {
            // Get all institutions (you might want to optimize this to only active institutions)
            var institutions = await GetActiveInstitutionsAsync(scope.ServiceProvider, cancellationToken);

            foreach (var institutionId in institutions)
            {
                try
                {
                    await UpdateInstitutionRoomProgress(
                        habitacionesService,
                        roomNotificationService,
                        institutionId,
                        cancellationToken
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error updating room progress for institution {InstitutionId}",
                        institutionId
                    );
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateAllRoomProgress");
        }
    }

    private async Task UpdateInstitutionRoomProgress(
        IHabitacionesService habitacionesService,
        IRoomNotificationService roomNotificationService,
        int institutionId,
        CancellationToken cancellationToken)
    {
        var occupiedRooms = await habitacionesService.GetOccupiedRoomsWithTimingAsync(
            institutionId,
            cancellationToken
        );

        var updatedCount = 0;

        foreach (var room in occupiedRooms)
        {
            if (room.VisitaId.HasValue && room.ReservationStartTime.HasValue)
            {
                var progress = CalculateProgress(room);

                await roomNotificationService.NotifyRoomProgressUpdated(
                    room.RoomId,
                    room.VisitaId.Value,
                    progress,
                    room.InstitutionId
                );

                updatedCount++;
            }
        }

        if (updatedCount > 0)
        {
            _logger.LogInformation(
                "Updated progress for {Count} occupied rooms in institution {InstitutionId}",
                updatedCount,
                institutionId
            );
        }
    }

    private RoomProgressDto CalculateProgress(OccupiedRoomDto room)
    {
        var startTime = room.ReservationStartTime!.Value;
        var currentTime = DateTime.UtcNow;
        var totalMinutes = room.TotalMinutes > 0 ? room.TotalMinutes : 60; // Default to 60 minutes if not specified

        var elapsedMinutes = (int)(currentTime - startTime).TotalMinutes;
        var progressPercentage = Math.Min((double)elapsedMinutes / totalMinutes * 100, 100);

        var hours = elapsedMinutes / 60;
        var minutes = elapsedMinutes % 60;
        var timeElapsed = $"{hours:D2}:{minutes:D2}";

        var estimatedEndTime = startTime.AddMinutes(totalMinutes);

        return new RoomProgressDto
        {
            StartTime = startTime,
            CurrentTime = currentTime,
            ProgressPercentage = Math.Round(progressPercentage, 1),
            TimeElapsed = timeElapsed,
            EstimatedEndTime = estimatedEndTime,
            TotalMinutes = totalMinutes,
            ElapsedMinutes = elapsedMinutes
        };
    }

    private async Task<List<int>> GetActiveInstitutionsAsync(
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        // For now, return a hardcoded list or implement logic to get active institutions
        // You might want to create an IInstitutionService to get this data
        
        // Temporary implementation - you should replace this with actual logic
        return await Task.FromResult(new List<int> { 1 }); // Default to institution 1
        
        // TODO: Implement actual logic to get active institutions
        // Example:
        // var institutionService = serviceProvider.GetRequiredService<IInstitutionService>();
        // return await institutionService.GetActiveInstitutionIdsAsync(cancellationToken);
    }
}