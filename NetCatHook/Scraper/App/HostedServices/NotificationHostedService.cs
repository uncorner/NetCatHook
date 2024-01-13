using NetCatHook.Scraper.App.NotificationProviders;

namespace NetCatHook.Scraper.App.HostedServices;

class NotificationHostedService : IHostedService
{
    private readonly ILogger<NotificationHostedService> logger;
    private readonly WeatherNotifyer weatherNotifyer;
    private readonly INotificationProvider notificationProvider;

    public NotificationHostedService(ILogger<NotificationHostedService> logger,
        WeatherNotifyer weatherNotifyer,
        INotificationProvider notificationProvider)
    {
        this.logger = logger;
        this.weatherNotifyer = weatherNotifyer;
        this.notificationProvider = notificationProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting notification provider");
        await notificationProvider.Initialize(cancellationToken);

        if (!cancellationToken.IsCancellationRequested)
        {
            weatherNotifyer.Event += HandleWeatherNotifyer;
        }
    }

    private async void HandleWeatherNotifyer(string message)
    {
        try
        {
            await notificationProvider.SendMessage(message);
        }
        catch(Exception ex)
        {
            logger.LogError($"Error send notification: {ex.Message}, Inner: {ex.InnerException?.Message}");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        weatherNotifyer.Event -= HandleWeatherNotifyer;
        notificationProvider.Dispose();

        logger.LogInformation("Notification provider stopped");
        return Task.CompletedTask;
    }

}
