using NetCatHook.Scraper.App.Parsing;
using NetCatHook.Scraper.Domain;

namespace NetCatHook.Scraper.App
{
    class TimeoutScheduler : IDisposable, IAsyncDisposable
    {
        public const string TargetUrl = "https://www.gismeteo.ru/weather-ryazan-4394/now/";
        private readonly ILogger<TimeoutScheduler> logger;
        private readonly IHtmlSource htmlSource;
        private readonly WeatherHtmlParser parser;
        private readonly WeatherNotifyer notifyer;
        private readonly Timer timer;

        public TimeoutScheduler(ILogger<TimeoutScheduler> logger,
            IHtmlSource htmlSource, WeatherHtmlParser parser, WeatherNotifyer notifyer)
        {
            timer = new Timer(new TimerCallback(Process));
            this.logger = logger;
            this.htmlSource = htmlSource;
            this.parser = parser;
            this.notifyer = notifyer;
        }

        public void Start(TimeSpan timeout)
        {
            timer.Change(TimeSpan.FromSeconds(5), timeout);
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

            var weatherData = parser.TryParse(html);
            if (weatherData.Processed)
            {
                var result = WeatherEvaluator.Evaluate(weatherData);
                if (result.Processed && result.TextMessage is not null)
                {
                    notifyer.SendMessage(result.TextMessage);
                    logger.LogInformation("Weather message was sent to Tg chats");
                }
                else
                {
                    logger.LogInformation("Weather message was NOT sent to Tg chats");
                }
            }
            else
            {
                logger.LogError("Parsing failed");
                return;
            }
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
