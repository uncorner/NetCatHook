using NetCatHook.Scraper.App.Entities;

namespace NetCatHook.Scraper.App.NotificationProviders;

interface INotificationProvider : IDisposable
{
    public Task Initialize(CancellationToken cancellationToken);

    public Task SendData(string? message, WeatherReport weatherReport);
}
