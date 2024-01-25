using NetCatHook.Scraper.App.Entities;
using NetCatHook.Scraper.App.HtmlProcessing;
using NetCatHook.Scraper.App.NotificationProviders;
using NetCatHook.Scraper.App.Repository;
using NetCatHook.Scraper.App.Schedulers;

namespace NetCatHook.Scraper.App.HostedServices;

class MessengerHostedService : IHostedService
{
    private readonly ILogger<MessengerHostedService> logger;
    private readonly IWorkScheduler scheduler;
    private readonly INotificationProvider notificationProvider;
    private readonly IUnitOfWorkFactory unitOfWorkFactory;
    private readonly IHtmlSource htmlSource;
    private readonly IWeatherHtmlParser parser;
    private readonly IConfiguration config;

    public MessengerHostedService(
        ILogger<MessengerHostedService> logger,
        IWorkScheduler scheduler,
        INotificationProvider notificationProvider,
        IUnitOfWorkFactory unitOfWorkFactory,
        IHtmlSource htmlSource, IWeatherHtmlParser parser,
        IConfiguration config)
    {
        this.logger = logger;
        this.scheduler = scheduler;
        this.notificationProvider = notificationProvider;
        this.unitOfWorkFactory = unitOfWorkFactory;
        this.htmlSource = htmlSource;
        this.parser = parser;
        this.config = config;
    }

    private async void ProcessScheduler()
    {
        logger.LogInformation("Start building weather report");
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
                var sendingTask = notificationProvider.SendData(sendingMessage, report);
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
            logger.LogError($"Weather report failed. Error: {ex.Message}, Inner: {ex.InnerException?.Message ?? "none"}");
        }
    }

    private static int GetRandomSlowMoInSec()
    {
        var random = new Random();
        return random.Next(30, 100);
    }

    private async Task SaveWeatherReport(WeatherReport report)
    {
        await using var unitOfWork = unitOfWorkFactory.CreateUnitOfWork();
        var reportRepository = unitOfWork.CreateWeatherReportRepository();
        await reportRepository.Add(report);

        await unitOfWork.SaveChangesAsync();
    }

    #region IHostedService
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation($"Start {nameof(MessengerHostedService)}");
        await notificationProvider.Initialize(cancellationToken);
        
        scheduler.Event += ProcessScheduler;
        scheduler.Start();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation($"Stop {nameof(MessengerHostedService)}");
        scheduler.Event -= ProcessScheduler;
        scheduler.Dispose();
        notificationProvider.Dispose();

        return Task.CompletedTask;
    }
    #endregion

}
