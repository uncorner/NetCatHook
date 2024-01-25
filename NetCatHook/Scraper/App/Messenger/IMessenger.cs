using NetCatHook.Scraper.App.Entities;

namespace NetCatHook.Scraper.App.Messenger;

interface IMessenger : IDisposable
{
    public Task Initialize(CancellationToken cancellationToken);

    public Task SendData(string? message, WeatherReport weatherReport);
}
