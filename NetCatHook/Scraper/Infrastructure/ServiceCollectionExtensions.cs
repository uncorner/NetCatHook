using Microsoft.Extensions.Logging;
using NetCatHook.Scraper.App;
using NetCatHook.Scraper.App.FetchingSchedulers;
using NetCatHook.Scraper.App.HostedServices;
using NetCatHook.Scraper.App.HtmlProcessing;
using NetCatHook.Scraper.App.NotificationProviders;
using NetCatHook.Scraper.App.Parsing;
using NetCatHook.Scraper.App.Repository;
using NetCatHook.Scraper.App.Telegram;
using NetCatHook.Scraper.Infrastructure.HtmlProcessing;
using NetCatHook.Scraper.Infrastructure.Repository;

namespace NetCatHook.Scraper.Infrastructure;

static class ServiceCollectionExtensions
{
    public static void AddCustomServices(this IServiceCollection services,
        ConfigurationManager config)
    {
        services.AddTransient<IUnitOfWorkFactory, UnitOfWorkFactory>();
        services.AddDbContextFactory<ApplicationDbContext>();

        services.AddSingleton<WeatherNotifyer>();
        AddHtmlDownloader(services, config);

        services.AddTransient<IWeatherHtmlParser, WeatherHtmlParser>();
        services.AddTransient<IFetchingScheduler, RandomTimeoutFetchingScheduler>();
        AddNotificationProvider(services, config);

        services.AddHostedService<NotificationHostedService>();
        services.AddHostedService<FetchingSchedulerHostedService>();
    }

    private static void AddNotificationProvider(IServiceCollection services, ConfigurationManager config)
    {
        if (config.GetLoggerNotificationsEnabled())
        {
            services.AddTransient<INotificationProvider, LoggerNotificationProvider>();
        }
        else
        {
            services.AddHttpClient<TgBotNotificationProvider>();
            services.AddTransient<INotificationProvider, TgBotNotificationProvider>();
        }
    }

    private static void AddHtmlDownloader(IServiceCollection services, ConfigurationManager config)
    {
        if (config.GetFakeHtmlDownloaderEnabled())
        {
            services.AddTransient<IHtmlSource,
                FakeHtmlDownloader>((service) => CreateFakeHtmlDownloader(service, string.Empty));
        }
        else
        {
            services.AddTransient<IHtmlSource, BrowserHtmlDownloader>();
        }
    }

    private static FakeHtmlDownloader CreateFakeHtmlDownloader(IServiceProvider services, string html)
    {
        var logger = services.GetRequiredService<ILogger<FakeHtmlDownloader>>();
        logger.LogInformation($"Using {nameof(FakeHtmlDownloader)}");
        return new FakeHtmlDownloader(logger, html);
    }

}
