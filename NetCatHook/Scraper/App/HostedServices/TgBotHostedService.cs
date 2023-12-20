namespace NetCatHook.Scraper.App.HostedServices;

class TgBotHostedService : IHostedService
{
    private readonly ILogger<TgBotHostedService> logger;

    public TgBotHostedService(ILogger<TgBotHostedService> logger)
    {
        this.logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Start TgBotHostedService and waiting 5 seconds...");
        await Task.Delay(5000);
        logger.LogInformation("TgBotHostedService started");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        //todo
        logger.LogInformation("TgBotHostedService stopped");
        return Task.CompletedTask;
    }

}
