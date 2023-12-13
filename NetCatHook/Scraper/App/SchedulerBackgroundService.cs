namespace NetCatHook.Scraper.App
{
    class SchedulerBackgroundService : BackgroundService
    {
        private readonly SimpleScheduler scheduler;

        public SchedulerBackgroundService(SimpleScheduler scheduler)
        {
            this.scheduler = scheduler;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!stoppingToken.IsCancellationRequested)
            {
                scheduler.Start(TimeSpan.FromHours(2));
            }

            stoppingToken.Register(scheduler.Dispose);

            return Task.CompletedTask;
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
