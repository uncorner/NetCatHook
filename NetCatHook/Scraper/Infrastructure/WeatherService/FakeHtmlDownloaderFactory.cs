using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NetCatHook.Scraper.Infrastructure.WeatherService;

static class FakeHtmlDownloaderFactory
{
    private const string FakeWeatherPageFile = @"c:\NetCatHook_TestData\weather_page.html";

    public static FakeHtmlDownloader FromFile(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<FakeHtmlDownloader>>();
        logger.LogInformation($"Using {nameof(FakeHtmlDownloader)}");

        var html = LoadHtmlFromFile(logger);
        return new FakeHtmlDownloader(logger, html);
    }

    private static string LoadHtmlFromFile(ILogger<FakeHtmlDownloader> logger)
    {
        if (!File.Exists(FakeWeatherPageFile))
        {
            logger.LogError($"File not exists: {FakeWeatherPageFile}");
            return string.Empty;
        }

        return File.ReadAllText(FakeWeatherPageFile);
    }

}
