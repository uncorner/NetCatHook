using NetCatHook.Scraper.App.Entities;
using NetCatHook.Scraper.App.HtmlProcessing;
using NetCatHook.Scraper.App.Repository;

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

                var report = new WeatherReport();
                report.SetWeatherData(weatherData);
                report.CreatedAtLocal = DateTime.Now;
                 
                var evalResult = WeatherEvaluator.Evaluate(weatherData);
                string? sendingMessage = null;
                if (evalResult.Processed && evalResult.TextMessage is not null)
                {
                    sendingMessage = evalResult.TextMessage;
                    logger.LogInformation("Weather notification message is sending");
                }
                else
                {
                    logger.LogError("Weather data is not evaluated");
                }

                var savingTask = SaveWeatherReport(report);
                var sendingTask = Task.Run(() => notifyer.SendData(sendingMessage, report));
                await Task.WhenAll(savingTask, sendingTask);
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

    private async Task SaveWeatherReport(WeatherReport report)
    {
        await using var unitOfWork = unitOfWorkFactory.CreateUnitOfWork();
        var reportRepository = unitOfWork.CreateWeatherReportRepository();
        //var report = new WeatherReport();
        //report.SetWeatherData(weatherData);
        //report.CreatedAtLocal = DateTime.Now;
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