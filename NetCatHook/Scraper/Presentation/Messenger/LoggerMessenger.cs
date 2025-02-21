using Microsoft.Extensions.Logging;
using NetCatHook.Scraper.Application;

namespace NetCatHook.Scraper.Presentation.Messenger;

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

    public Task Send(string? message)
    {
        logger.LogInformation($"Notification message: {message}");
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        //do nothing
    }

    public void SetWeatherInformer(IWeatherInformer weatherInformer)
    {
        //do nothing
    }
}
