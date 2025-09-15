
using Cronos;

namespace hotel.Jobs;


public abstract class CronBackgroundJob : BackgroundService
{
    private PeriodicTimer? _timer;
    private readonly CronExpression _cronExpression;
    private readonly TimeZoneInfo _timeZone;

    public CronBackgroundJob(string rawCronExpression, TimeZoneInfo timeZone)
    {
        _cronExpression = CronExpression.Parse(rawCronExpression);
        _timeZone = timeZone;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                DateTimeOffset? nextOcurrence = _cronExpression.GetNextOccurrence(DateTimeOffset.UtcNow, _timeZone);

                if (nextOcurrence.HasValue)
                {
                    var delay = nextOcurrence.Value - DateTimeOffset.UtcNow;
                    
                    if (delay > TimeSpan.Zero)
                    {
                        _timer = new PeriodicTimer(delay);

                        try
                        {
                            if (await _timer.WaitForNextTickAsync(stoppingToken))
                            {
                                await DoWork(stoppingToken);
                            }
                        }
                        finally
                        {
                            _timer?.Dispose();
                            _timer = null;
                        }
                    }
                }
                else
                {
                    // No hay próxima ocurrencia, esperamos un tiempo antes de verificar nuevamente
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Es esperado cuando la aplicación se cierra
                break;
            }
        }
    }

    protected abstract Task DoWork(CancellationToken stoppingToken);
}