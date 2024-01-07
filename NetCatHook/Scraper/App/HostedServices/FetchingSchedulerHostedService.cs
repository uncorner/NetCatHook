namespace NetCatHook.Scraper.App.HostedServices;

class FetchingSchedulerHostedService : BackgroundService
{
    private readonly IFetchingScheduler scheduler;
    private bool isDisposed = false;

    public FetchingSchedulerHostedService(IFetchingScheduler scheduler)
    {
        this.scheduler = scheduler;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (stoppingToken.IsCancellationRequested)
        {
            return Task.CompletedTask;
        }

        scheduler.Start();

        return Task.CompletedTask;
    }

    #region Dispose
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
