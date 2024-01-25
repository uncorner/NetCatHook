using NetCatHook.Scraper.App.Entities;

namespace NetCatHook.Scraper.App.Messenger;

class LoggerMessenger : IMessenger
{
    private readonly ILogger<LoggerMessenger> logger;

    public LoggerMessenger(ILogger<LoggerMessenger> logger)
    {
        this.logger = logger;
    }

    public Task Initialize(CancellationToken cancellationToken)
    {
        logger.LogInformation("Init logger messenger");
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
