namespace NetCatHook.Scraper.App
{
    class SimpleScheduler : IDisposable, IAsyncDisposable
    {
        private readonly ILogger<SimpleScheduler> logger;
        private readonly TimeSpan period;
        private readonly Timer timer;

        public SimpleScheduler(ILogger<SimpleScheduler> logger, TimeSpan period)
        {
            timer = new Timer(new TimerCallback(Process));
            this.logger = logger;
            this.period = period;
        }

        public void Start()
        {
            timer.Change(TimeSpan.FromSeconds(5), period);
        }

        private void Process(object? state)
        {
            //todo
            logger.LogInformation(">>> Timer Process");
        }

        #region Dispose, IAsyncDisposable
        private bool isDisposed = false;

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            timer.Dispose();
            isDisposed = true;
        }

        public async ValueTask DisposeAsync()
        {
            await Task.Run(Dispose);
        }
        #endregion

    }
}
