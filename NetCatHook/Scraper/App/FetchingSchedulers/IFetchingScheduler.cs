namespace NetCatHook.Scraper.App.FetchingSchedulers;

interface IFetchingScheduler : IDisposable, IAsyncDisposable
{
    public void Start();

}
