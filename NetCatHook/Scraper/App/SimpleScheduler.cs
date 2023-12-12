
using NetCatHook.Scraper.App.Parsing;

namespace NetCatHook.Scraper.App
{
    class SimpleScheduler : IDisposable, IAsyncDisposable
    {
        public const string TargetUrl = "https://www.gismeteo.ru/weather-ryazan-4394/now/";
        private readonly ILogger<SimpleScheduler> logger;
        private readonly TimeSpan period;
        private readonly IHtmlSource htmlSource;
        private readonly Timer timer;

        public SimpleScheduler(ILogger<SimpleScheduler> logger, TimeSpan period,
            IHtmlSource htmlSource)
        {
            timer = new Timer(new TimerCallback(Process));
            this.logger = logger;
            this.period = period;
            this.htmlSource = htmlSource;
        }

        public void Start()
        {
            timer.Change(TimeSpan.FromSeconds(5), period);
        }

        private void Process(object? state)
        {
            logger.LogInformation("Start processing");
            var html = htmlSource.GetHtmlDataAsync(TargetUrl).Result;
            if (html is null)
            {
                logger.LogError("html is null");
                return;
            }

            var parser = new WeatherHtmlParser(html);
            var result = parser.TryParseAsync().Result;

            if (!result.success)
            {
                logger.LogError("Parsing failed");
                return;
            }
            
            logger.LogInformation($"Success parsing: {result.temp}");
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
