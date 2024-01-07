using NetCatHook.Scraper.App.HtmlProcessing;
using NetCatHook.Scraper.Domain;

namespace NetCatHook.Scraper.App;

class RandomTimeoutScheduler : IDisposable, IAsyncDisposable
{
    private readonly ILogger<TimeoutScheduler> logger;
    private readonly IHtmlSource htmlSource;
    private readonly IWeatherHtmlParser parser;
    private readonly WeatherNotifyer notifyer;
    private readonly IConfiguration config;
    private readonly Timer timer;
    private readonly int timeoutBase;
    private bool isDisposed = false;
    private readonly object syncObj = new();

    public RandomTimeoutScheduler(ILogger<TimeoutScheduler> logger,
        IHtmlSource htmlSource, IWeatherHtmlParser parser,
        WeatherNotifyer notifyer, IConfiguration config)
    {
        timer = new Timer(new TimerCallback(Process));

        this.logger = logger;
        this.htmlSource = htmlSource;
        this.parser = parser;
        this.notifyer = notifyer;
        this.config = config;
        timeoutBase = config.GetParsingSchedulerTimeoutInMinutes();
    }

    public void Start()
    {
        StartTimerNoRepeat(TimeSpan.Zero);
    }

    private void StartTimerNoRepeat(TimeSpan dueTime)
    {
        timer.Change(dueTime, Timeout.InfiniteTimeSpan);
    }

    private async void Process(object? state)
    {
        logger.LogInformation("Start html parsing");
        try
        {
            var parsingUrl = config.GetWeatherParsingUrl();
            logger.LogInformation($"Parsing for URL: {parsingUrl}");

            htmlSource.SlowMo = GetRandomSlowMoInSec();
            logger.LogInformation($"Browser SlowMo timout is {htmlSource.SlowMo} sec");

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
            logger.LogError($"Html parsing failed. Error: {ex.Message}, Inner: {ex.InnerException?.Message ?? "none"}");
        }
        finally
        {
            SafelyRestartTimer();
        }
    }

    private void SafelyRestartTimer()
    {
        if (!isDisposed)
        {
            lock (syncObj)
            {
                if (!isDisposed)
                {
                    var randTimeout = GetRandomTimeoutInMinutes();
                    logger.LogInformation($"Parsing scheduler started with timeout {randTimeout} minutes");
                    StartTimerNoRepeat(TimeSpan.FromMinutes(randTimeout));
                }
            }
        }
    }

    private int GetRandomTimeoutInMinutes()
    {
        var range = timeoutBase / 4;
        var random = new Random();
        var randValueX2 = random.Next(range * 2);
        var signedRandValue = range - randValueX2;
        return timeoutBase + signedRandValue;
    }

    private static int GetRandomSlowMoInSec()
    {
        var random = new Random();
        return random.Next(30, 100);
    }

    #region Dispose, IAsyncDisposable
    public void Dispose()
    {
        if (isDisposed)
        {
            return;
        }

        lock (syncObj)
        {
            timer.Dispose();
            isDisposed = true;
            logger.LogInformation("Random timeout scheduler disposed");
        }
    }

    public async ValueTask DisposeAsync()
    {
        await Task.Run(Dispose);
    }
    #endregion

}