namespace NetCatHook.Scraper.App.HostedServices;

class SchedulerHostedService : BackgroundService
{
    private readonly RandomTimeoutScheduler scheduler;
    private readonly IConfiguration configuration;
    private readonly ILogger<SchedulerHostedService> logger;

    public SchedulerHostedService(RandomTimeoutScheduler scheduler, IConfiguration configuration,
        ILogger<SchedulerHostedService> logger)
    {
        this.scheduler = scheduler;
        this.configuration = configuration;
        this.logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (stoppingToken.IsCancellationRequested)
        {
            return Task.CompletedTask;
        }

        var timeout = configuration.GetParsingSchedulerTimeoutInMinutes();
        scheduler.Start();
        //logger.LogInformation($"Parsing scheduler starter with timeout {timeout} minutes");

        return Task.CompletedTask;
    }

    #region Dispose
    private bool isDisposed = false;

    public override void Dispose()
    {
        if (isDisposed)
        {
            return;
        }

        scheduler.Dispose();
        base.Dispose();
        isDisposed = true;
    }
    #endregion

}
