using NetCatHook.Scraper.App;
using NetCatHook.Scraper.App.HostedServices;
using NetCatHook.Scraper.App.HtmlProcessing;
using NetCatHook.Scraper.App.Parsing;
using NetCatHook.Scraper.App.Repository;
using NetCatHook.Scraper.Infrastructure.HtmlProcessing;
using NetCatHook.Scraper.Infrastructure.Repository;

namespace NetCatHook.Scraper.Infrastructure;

static class ServiceCollectionExtensions
{
    public static void AddCustomServices(this IServiceCollection services,
        ConfigurationManager configuration)
    {
        services.AddTransient<IUnitOfWorkFactory, UnitOfWorkFactory>();
        services.AddDbContextFactory<ApplicationDbContext>();

        services.AddHttpClient<TgBotHostedService>();
        services.AddSingleton<WeatherNotifyer>();

        //services.AddTransient<IHtmlSource, ChromeHtmlDownloader>();

        services.AddTransient<IHtmlSource, FakeHtmlDownloader>(CreateFakeHtmlDownloader);
        services.AddTransient<IWeatherHtmlParser, WeatherHtmlParser>();
        services.AddTransient<RandomTimeoutScheduler>();

        //services.AddHostedService<TgBotHostedService>();

        services.AddHostedService<SchedulerHostedService>();
    }

    private static FakeHtmlDownloader CreateFakeHtmlDownloader(IServiceProvider service)
    {
        var logger = service.GetRequiredService<ILogger<FakeHtmlDownloader>>();
        return new FakeHtmlDownloader(logger, "");
    }

}
