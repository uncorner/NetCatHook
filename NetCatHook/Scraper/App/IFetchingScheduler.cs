namespace NetCatHook.Scraper.App;

interface IFetchingScheduler : IDisposable, IAsyncDisposable
{
    public void Start();

}
