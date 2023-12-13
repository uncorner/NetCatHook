
using NetCatHook.Scraper.App.Parsing;

namespace NetCatHook.Scraper.App
{
    class SimpleScheduler : IDisposable, IAsyncDisposable
    {
        public const string TargetUrl = "https://www.gismeteo.ru/weather-ryazan-4394/now/";
        private readonly ILogger<SimpleScheduler> logger;
        private readonly IHtmlSource htmlSource;
        private readonly WeatherHtmlParser parser;
        private readonly Timer timer;

        public SimpleScheduler(ILogger<SimpleScheduler> logger,
            IHtmlSource htmlSource, WeatherHtmlParser parser)
        {
            timer = new Timer(new TimerCallback(Process));
            this.logger = logger;
            this.htmlSource = htmlSource;
            this.parser = parser;
        }

        public void Start(TimeSpan period)
        {
            timer.Change(TimeSpan.Zero, period);
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

            var result = parser.TryParse(html);
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
            logger.LogInformation("disposed");
        }

        public async ValueTask DisposeAsync()
        {
            await Task.Run(Dispose);
        }
        #endregion

    }
}
