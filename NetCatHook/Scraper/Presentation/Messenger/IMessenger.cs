using NetCatHook.Scraper.Application;

namespace NetCatHook.Scraper.Presentation.Messenger;

interface IMessenger : IDisposable
{
    public Task Initialize(CancellationToken cancellationToken);

    public void SetWeatherInformer(IWeatherInformer weatherInformer);

    public Task Send(string message);
}
