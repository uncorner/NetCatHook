using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetCatHook.Scraper.Application;
using NetCatHook.Scraper.Application.Messenger;
using NetCatHook.Scraper.Application.Repository;
using NetCatHook.Scraper.Application.Schedulers;
using NetCatHook.Scraper.Application.WeatherService;
using NetCatHook.Scraper.Infrastructure.Repository;
using NetCatHook.Scraper.Infrastructure.Schedulers;
using NetCatHook.Scraper.Infrastructure.WeatherService;
using NetCatHook.Scraper.Presentation.Messenger;

namespace NetCatHook.Scraper.Infrastructure;

static class ServiceCollectionExtensions
{
    public static void AddCustomServices(this IServiceCollection services,
        ConfigurationManager config)
    {
        services.AddHttpClient();

        services.AddTransient<IUnitOfWorkFactory, DbUnitOfWorkFactory>();
        //NOTE: for testing
        //services.AddTransient<IUnitOfWorkFactory, MemoryUnitOfWorkFactory>();
        services.AddDbContextFactory<ApplicationDbContext>();

        AddHtmlSource(services, config);

        services.AddTransient<IWeatherHtmlParser, WeatherHtmlParser>();
        services.AddTransient<IWorkScheduler, RandomTimeoutScheduler>();
        AddMessengerByConfig(services, config);
    }

    private static void AddMessengerByConfig(IServiceCollection services, ConfigurationManager config)
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
