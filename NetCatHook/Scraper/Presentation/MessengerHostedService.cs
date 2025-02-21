using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetCatHook.Scraper.Application;
using NetCatHook.Scraper.Application.HtmlProcessing;
using NetCatHook.Scraper.Application.Messenger;
using NetCatHook.Scraper.Application.Repository;
using NetCatHook.Scraper.Application.Schedulers;
using NetCatHook.Scraper.Domain.Entities;

namespace NetCatHook.Scraper.Presentation;

class MessengerHostedService : IHostedService, IWeatherInformer
{
    private readonly ILogger<MessengerHostedService> logger;
    private readonly IWorkScheduler scheduler;
    private readonly IMessenger messenger;
    private readonly IUnitOfWorkFactory unitOfWorkFactory;
    private readonly IHtmlSource htmlSource;
    private readonly IWeatherHtmlParser parser;
    private readonly IConfiguration config;
    private WeatherReport? cachedLastWeatherReport;
    private readonly object reportSyncObject = new();

    public MessengerHostedService(
        ILogger<MessengerHostedService> logger,
        IWorkScheduler scheduler,
        IMessenger messenger,
        IUnitOfWorkFactory unitOfWorkFactory,
        IHtmlSource htmlSource, IWeatherHtmlParser parser,
        IConfiguration config)
    {
        this.logger = logger;
        this.scheduler = scheduler;
        this.messenger = messenger;
        this.unitOfWorkFactory = unitOfWorkFactory;
        this.htmlSource = htmlSource;
        this.parser = parser;
        this.config = config;

        this.messenger.SetWeatherInformer(this);
    }

    // NOTE: void because be used with event
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
                var savingReportTask = SaveWeatherReport(report);

                var evalResult = WeatherEvaluator.Evaluate(weatherData);
                var sendingTask = Task.CompletedTask;

                if (evalResult.Processed && evalResult.TextMessage is not null)
                {
                    sendingTask = messenger.Send(evalResult.TextMessage);
                    logger.LogInformation("Weather notification message is sending");
                }
                else
                {
                    logger.LogError("Weather data is not evaluated");
                }
                
                await Task.WhenAll(savingReportTask, sendingTask);
                SetSafeCachedLastWeatherReport(report);
            }
            else
            {
                logger.LogError("Parsing failed");
                return;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
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

    private WeatherReport GetSafeLastWeatherReport()
    {
        if (cachedLastWeatherReport is null)
        {
            lock (reportSyncObject)
            {
                if (cachedLastWeatherReport is null)
                {
                    cachedLastWeatherReport = LoadLastWeatherReport();
                    cachedLastWeatherReport ??= WeatherReport.CreateExpired();
                }
            }
        }

        return cachedLastWeatherReport;
    }

    private WeatherReport? LoadLastWeatherReport()
    {
        using var unitOfWork = unitOfWorkFactory.CreateUnitOfWork();
        var reportRepository = unitOfWork.CreateWeatherReportRepository();
        return reportRepository.GetLast().Result;
    }

    private void SetSafeCachedLastWeatherReport(WeatherReport report)
    {
        lock (reportSyncObject)
        {
            cachedLastWeatherReport = report;
        }
    }

    public string GetWeatherSummary()
    {
        var report = GetSafeLastWeatherReport();
        var timeoutMinutes = config.GetParsingSchedulerTimeoutInMinutes();
        var expiringTimeUtc = DateTime.UtcNow.AddMinutes(-timeoutMinutes * 2);

        var summary = "Пока нет данных о погоде";
        if (report.CreatedAt >= expiringTimeUtc)
        {
            summary = WeatherSummaryBuilder.Build(report);
        }

        return summary;
    }

    #region IHostedService

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation($"Start {nameof(MessengerHostedService)}");
        await messenger.Initialize(cancellationToken);
        
        scheduler.Event += ProcessScheduler;
        scheduler.Start();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation($"Stop {nameof(MessengerHostedService)}");
        scheduler.Event -= ProcessScheduler;
        scheduler.Dispose();
        messenger.Dispose();

        return Task.CompletedTask;
    }
    
    #endregion

}
