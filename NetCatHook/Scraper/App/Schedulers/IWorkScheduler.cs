namespace NetCatHook.Scraper.App.Schedulers;

internal delegate void SchedulerEventHandler();

interface IWorkScheduler : IDisposable, IAsyncDisposable
{
    public void Start();

    public event SchedulerEventHandler? Event;
}
