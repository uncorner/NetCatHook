using NetCatHook.Scraper.App.Entities;
using NetCatHook.Scraper.App.HtmlProcessing;
using NetCatHook.Scraper.App.Repository;
using NetCatHook.Scraper.Domain;

namespace NetCatHook.Scraper.App.FetchingSchedulers;

class RandomTimeoutFetchingScheduler : IFetchingScheduler
{
    private readonly ILogger<RandomTimeoutFetchingScheduler> logger;
    private readonly IHtmlSource htmlSource;
    private readonly IWeatherHtmlParser parser;
    private readonly WeatherNotifyer notifyer;
    private readonly IConfiguration config;
    private readonly IUnitOfWorkFactory unitOfWorkFactory;
    private readonly Timer timer;
    private readonly int timeoutBase;
    private bool isDisposed = false;
    private readonly object syncObj = new();

    public RandomTimeoutFetchingScheduler(
        ILogger<RandomTimeoutFetchingScheduler> logger,
        IHtmlSource htmlSource, IWeatherHtmlParser parser,
        WeatherNotifyer notifyer, IConfiguration config,
        IUnitOfWorkFactory unitOfWorkFactory)
    {
        timer = new Timer(new TimerCallback(Process));

        this.logger = logger;
        this.htmlSource = htmlSource;
        this.parser = parser;
        this.notifyer = notifyer;
        this.config = config;
        this.unitOfWorkFactory = unitOfWorkFactory;
        timeoutBase = config.GetParsingSchedulerTimeoutInMinutes();
    }

    public void Start()
    {
        if (isDisposed)
        {
            return;
        }

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
            logger.LogInformation($"Set SlowMo timout {htmlSource.SlowMo} sec");

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
                    var messageTask = Task.Run(() =>
                        notifyer.SendMessage(result.TextMessage));
                    var saveTask = SaveWeatherReport(weatherData);
                    await Task.WhenAll(messageTask, saveTask);

                    //notifyer.SendMessage(result.TextMessage);
                    //await SaveWeatherReport(weatherData);

                    logger.LogInformation("Weather notification message was sent");
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

    private async Task SaveWeatherReport(WeatherData weatherData)
    {
        await using var unitOfWork = unitOfWorkFactory.CreateUnitOfWork();
        var reportRepository = unitOfWork.CreateWeatherReportRepository();
        var report = new WeatherReport();
        report.SetWeatherData(weatherData);
        report.CreatedAtLocal = DateTime.Now;
        await reportRepository.Add(report);

        await unitOfWork.SaveChangesAsync();
    }

    private void SafelyRestartTimer()
    {
        if (isDisposed)
        {
            return;
        }

        lock (syncObj)
        {
            if (isDisposed)
            {
                return;
            }

            var randTimeout = GetRandomTimeoutInMinutes();
            logger.LogInformation($"Parsing scheduler started with timeout {randTimeout} minutes");
            StartTimerNoRepeat(TimeSpan.FromMinutes(randTimeout));
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
            logger.LogInformation("Fetching scheduler disposed");
        }
    }

    public async ValueTask DisposeAsync()
    {
        await Task.Run(Dispose);
    }
    #endregion

}