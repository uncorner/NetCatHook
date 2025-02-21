using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NetCatHook.Scraper.Application.Schedulers;

class TimeoutScheduler : IWorkScheduler
{
    private readonly ILogger<TimeoutScheduler> logger;
    private readonly IConfiguration config;
    private readonly Timer timer;
    private bool isDisposed = false;

    public event SchedulerEventHandler? Event;

    public TimeoutScheduler(
        ILogger<TimeoutScheduler> logger,
        IConfiguration config)
    {
        this.logger = logger;
        this.config = config;
        timer = new Timer(new TimerCallback(Process));
    }

    public void Start()
    {
        if (isDisposed)
        {
            return;
        }

        var timeout = config.GetParsingSchedulerTimeoutInMinutes();
        timer.Change(TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(timeout));
    }

    private void Process(object? state)
    {
        logger.LogError("Scheduler calls processing");
        try
        {
            Event?.Invoke();
        }
        catch (Exception ex)
        {
            logger.LogError($"Scheduler failed. Error: {ex.Message}, Inner: {ex.InnerException?.Message ?? "none"}");
        }
    }

    #region Dispose, IAsyncDisposable
    public void Dispose()
    {
        if (isDisposed)
        {
            return;
        }

        timer.Dispose();
        isDisposed = true;
        logger.LogInformation("Fetching scheduler disposed");
    }

    public async ValueTask DisposeAsync()
    {
        await Task.Run(Dispose);
    }
    #endregion

}
