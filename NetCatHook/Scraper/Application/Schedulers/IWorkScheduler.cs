namespace NetCatHook.Scraper.Application.Schedulers;

internal delegate void SchedulerEventHandler();

interface IWorkScheduler : IDisposable, IAsyncDisposable
{
    public void Start();

    public event SchedulerEventHandler? Event;
}
