namespace NetCatHook.Scraper.App.Messenger;

interface IMessenger : IDisposable
{
    public Task Initialize(CancellationToken cancellationToken);

    public void SetWeatherInformer(IWeatherInformer weatherInformer);

    public Task Send(string message);
}
