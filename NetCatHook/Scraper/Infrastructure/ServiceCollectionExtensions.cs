using Microsoft.EntityFrameworkCore;
using NetCatHook.Scraper.App;
using NetCatHook.Scraper.App.HostedServices;
using NetCatHook.Scraper.App.Parsing;
using NetCatHook.Scraper.App.Repository;
using NetCatHook.Scraper.Infrastructure.Repository;

namespace NetCatHook.Scraper.Infrastructure;

static class ServiceCollectionExtensions
{
    public static void AddCustomServices(this IServiceCollection services,
        ConfigurationManager configuration)
    {
        services.AddTransient<IUnitOfWorkFactory, UnitOfWorkFactory>();

        // TODO
        var connectionString = configuration.GetConnectionString("Default")!;
        services.AddDbContextFactory<ApplicationDbContext>(opt => opt.UseNpgsql(connectionString));

        services.AddHttpClient<TgBotHostedService>();
        services.AddSingleton<WeatherNotifyer>();

        services.AddTransient<IHtmlSource, HtmlDownloader>();
        services.AddTransient<WeatherHtmlParser>();
        services.AddTransient<SimpleScheduler>();

        services.AddHostedService<TgBotHostedService>();
        services.AddHostedService<SchedulerHostedService>();
    }


}
