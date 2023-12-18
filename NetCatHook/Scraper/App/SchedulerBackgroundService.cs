namespace NetCatHook.Scraper.App
{
    class SchedulerBackgroundService : BackgroundService
    {
        private readonly SimpleScheduler scheduler;

        public SchedulerBackgroundService(SimpleScheduler scheduler)
        {
            this.scheduler = scheduler;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(() =>
            {
                stoppingToken.Register(scheduler.Dispose);
                // todo: take it from config
                scheduler.Start(TimeSpan.FromHours(2));
            }, stoppingToken);
        }

        #region Dispose
        private bool isDisposed = false;

        public override void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            scheduler.Dispose();
            base.Dispose();
            isDisposed = true;
        }
        #endregion

    }
}
