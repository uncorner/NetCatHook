using NetCatHook.Scraper.App;
using NetCatHook.Scraper.App.HostedServices;
using NetCatHook.Scraper.App.HtmlProcessing;
using NetCatHook.Scraper.App.Messenger;
using NetCatHook.Scraper.App.Repository;
using NetCatHook.Scraper.App.Schedulers;
using NetCatHook.Scraper.Infrastructure.HtmlProcessing;
using NetCatHook.Scraper.Infrastructure.Messenger;
using NetCatHook.Scraper.Infrastructure.Repository;

namespace NetCatHook.Scraper.Infrastructure;

static class ServiceCollectionExtensions
{
    public static void AddCustomServices(this IServiceCollection services,
        ConfigurationManager config)
    {
        services.AddHttpClient();

        services.AddTransient<IUnitOfWorkFactory, DbUnitOfWorkFactory>();
        //services.AddTransient<IUnitOfWorkFactory, MemoryUnitOfWorkFactory>();
        services.AddDbContextFactory<ApplicationDbContext>();

        AddHtmlSource(services, config);

        services.AddTransient<IWeatherHtmlParser, WeatherHtmlParser>();
        services.AddTransient<IWorkScheduler, RandomTimeoutScheduler>();
        AddMessenger(services, config);

        services.AddHostedService<MessengerHostedService>();
    }

    private static void AddMessenger(IServiceCollection services, ConfigurationManager config)
    {
        if (config.GetLoggerNotificationsEnabled())
        {
            services.AddTransient<IMessenger, LoggerMessenger>();
        }
        else
        {
            services.AddTransient<IMessenger, TgBotMessenger>();
        }
    }

    private static void AddHtmlSource(IServiceCollection services, ConfigurationManager config)
    {
        var downloaderType = config.GetHtmlDownloaderType();
        switch(downloaderType)
        {
            case HtmlDownloaderTypes.Native:
                services.AddTransient<IHtmlSource, NativeHtmlDownloader>();
                break;
            case HtmlDownloaderTypes.Browser:
                services.AddTransient<IHtmlSource, BrowserHtmlDownloader>();
                break;
            default:
                services.AddTransient<IHtmlSource,
                    FakeHtmlDownloader>((services) => FakeHtmlDownloaderFactory.FromFile(services));
                break;
        }
    }

}
