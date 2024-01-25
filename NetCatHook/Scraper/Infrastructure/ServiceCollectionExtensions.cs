using NetCatHook.Scraper.App;
using NetCatHook.Scraper.App.HostedServices;
using NetCatHook.Scraper.App.HtmlProcessing;
using NetCatHook.Scraper.App.NotificationProviders;
using NetCatHook.Scraper.App.Parsing;
using NetCatHook.Scraper.App.Repository;
using NetCatHook.Scraper.App.Schedulers;
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

        AddHtmlSource(services, config);

        services.AddTransient<IWeatherHtmlParser, WeatherHtmlParser>();
        services.AddTransient<IWorkScheduler, RandomTimeoutScheduler>();
        AddNotificationProvider(services, config);

        services.AddHostedService<MessengerHostedService>();
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

    private static void AddHtmlSource(IServiceCollection services, ConfigurationManager config)
    {
        if (config.GetFakeHtmlDownloaderEnabled())
        {
            services.AddTransient<IHtmlSource,
                FakeHtmlDownloader>((services) => FakeHtmlDownloaderFactory.FromFile(services));
        }
        else
        {
            services.AddTransient<IHtmlSource, BrowserHtmlDownloader>();
        }
    }

}
