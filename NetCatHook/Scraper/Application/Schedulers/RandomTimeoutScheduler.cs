using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NetCatHook.Scraper.Application.Schedulers;

class RandomTimeoutScheduler : IWorkScheduler
{
    private readonly ILogger<RandomTimeoutScheduler> logger;
    private readonly Timer timer;
    private readonly int timeoutBase;
    private bool isDisposed = false;
    private readonly object syncObj = new();

    public event SchedulerEventHandler? Event;

    public RandomTimeoutScheduler(
        ILogger<RandomTimeoutScheduler> logger,
        IConfiguration config)
    {
        this.logger = logger;
        timeoutBase = config.GetParsingSchedulerTimeoutInMinutes();
        timer = new Timer(new TimerCallback(Process));
    }

    public void Start()
    {
        if (isDisposed)
        {
            return;
        }

        StartTimerNoRepeat(TimeSpan.Zero);
    }

    private void StartTimerNoRepeat(TimeSpan dueTime)
    {
        timer.Change(dueTime, Timeout.InfiniteTimeSpan);
    }

    private void Process(object? state)
    {
        logger.LogInformation("Scheduler calls processing");
        try
        {
            Event?.Invoke();
        }
        catch (Exception ex)
        {
            logger.LogError($"Scheduler failed. Error: {ex.Message}, Inner: {ex.InnerException?.Message ?? "none"}");
        }
        finally
        {
            SafelyRestartTimer();
        }
    }

    private void SafelyRestartTimer()
    {
        if (isDisposed)
        {
            return;
        }

        lock (syncObj)
        {
            if (isDisposed)
            {
                return;
            }

            var randTimeout = GetRandomTimeoutInMinutes();
            logger.LogInformation($"Scheduler started with timeout {randTimeout} minutes");
            StartTimerNoRepeat(TimeSpan.FromMinutes(randTimeout));
        }
    }

    private int GetRandomTimeoutInMinutes()
    {
        var range = timeoutBase / 4;
        var random = new Random();
        var randValueX2 = random.Next(range * 2);
        var signedRandValue = range - randValueX2;
        return timeoutBase + signedRandValue;
    }

    #region Dispose, IAsyncDisposable
    public void Dispose()
    {
        if (isDisposed)
        {
            return;
        }

        lock (syncObj)
        {
            timer.Dispose();
            isDisposed = true;
            logger.LogInformation("Scheduler disposed");
        }
    }

    public async ValueTask DisposeAsync()
    {
        await Task.Run(Dispose);
    }
    #endregion

}