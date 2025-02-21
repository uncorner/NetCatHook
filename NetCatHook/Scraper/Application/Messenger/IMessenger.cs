namespace NetCatHook.Scraper.Application.Messenger;

interface IMessenger : IDisposable
{
    public Task Initialize(CancellationToken cancellationToken);

    public void SetWeatherInformer(IWeatherInformer weatherInformer);

    public Task Send(string message);
}
