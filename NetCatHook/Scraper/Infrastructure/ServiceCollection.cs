using NetCatHook.Scraper.App;
using NetCatHook.Scraper.App.Parsing;

namespace NetCatHook.Scraper.Infrastructure;

static class ServiceCollection
{
    public static void AddCustomServices(this IServiceCollection services)
    {
        services.AddTransient<IHtmlSource, HtmlDownloader>();
        services.AddTransient<WeatherHtmlParser>();
        services.AddTransient<SimpleScheduler>();
    }

}
