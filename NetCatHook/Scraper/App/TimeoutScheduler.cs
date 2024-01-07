using NetCatHook.Scraper.App.HtmlProcessing;
using NetCatHook.Scraper.Domain;

namespace NetCatHook.Scraper.App;

class TimeoutScheduler : IDisposable, IAsyncDisposable
{
    private readonly ILogger<TimeoutScheduler> logger;
    private readonly IHtmlSource htmlSource;
    private readonly IWeatherHtmlParser parser;
    private readonly WeatherNotifyer notifyer;
    private readonly IConfiguration config;
    private readonly Timer timer;

    public TimeoutScheduler(ILogger<TimeoutScheduler> logger,
        IHtmlSource htmlSource, IWeatherHtmlParser parser,
        WeatherNotifyer notifyer, IConfiguration config)
    {
        timer = new Timer(new TimerCallback(Process));
        this.logger = logger;
        this.htmlSource = htmlSource;
        this.parser = parser;
        this.notifyer = notifyer;
        this.config = config;
    }

    public void Start(TimeSpan timeout)
    {
        timer.Change(TimeSpan.FromSeconds(5), timeout);
    }

    private async void Process(object? state)
    {
        logger.LogInformation("Start html parsing");
        try
        {
            var parsingUrl = config.GetWeatherParsingUrl();
            logger.LogInformation($"Parsing for URL: {parsingUrl}");
            var html = await htmlSource.GetHtmlDataAsync(parsingUrl);
            if (html is null)
            {
                throw new NullReferenceException("html is null");
            }

            var weatherData = await parser.TryParseAsync(html);
            if (weatherData.Processed)
            {
                logger.LogInformation("Parsing html succeeded");
                var result = WeatherEvaluator.Evaluate(weatherData);

                if (result.Processed && result.TextMessage is not null)
                {
                    notifyer.SendMessage(result.TextMessage);
                    logger.LogInformation("Weather message was sent to Tg chats");
                }
                else
                {
                    logger.LogError("Weather data is not processed");
                }
            }
            else
            {
                logger.LogError("Parsing failed");
                return;
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"Parsing failed. Error: {ex.Message}, Inner: {ex.InnerException?.Message ?? "none"}");
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
