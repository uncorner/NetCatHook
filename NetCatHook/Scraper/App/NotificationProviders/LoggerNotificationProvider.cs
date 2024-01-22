using NetCatHook.Scraper.App.Entities;

namespace NetCatHook.Scraper.App.NotificationProviders;

class LoggerNotificationProvider : INotificationProvider
{
    private readonly ILogger<LoggerNotificationProvider> logger;

    public LoggerNotificationProvider(ILogger<LoggerNotificationProvider> logger)
    {
        this.logger = logger;
    }

    public Task Initialize(CancellationToken cancellationToken)
    {
        logger.LogInformation("Init logger notifications");
        return Task.CompletedTask;
    }

    public Task SendData(string? message, WeatherReport weatherReport)
    {
        logger.LogInformation($"Notification message: {message}");
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        //do nothing
    }
    
}
