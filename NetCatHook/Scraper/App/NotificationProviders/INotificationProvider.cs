namespace NetCatHook.Scraper.App.NotificationProviders;

interface INotificationProvider : IDisposable
{
    public Task Initialize(CancellationToken cancellationToken);

    public Task SendMessage(string message);
}
