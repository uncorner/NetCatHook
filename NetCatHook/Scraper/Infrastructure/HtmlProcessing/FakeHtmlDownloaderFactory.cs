namespace NetCatHook.Scraper.Infrastructure.HtmlProcessing;

static class FakeHtmlDownloaderFactory
{
    public static FakeHtmlDownloader FromFile(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<FakeHtmlDownloader>>();
        logger.LogInformation($"Using {nameof(FakeHtmlDownloader)}");

        var html = LoadHtmlFromFile(logger);
        return new FakeHtmlDownloader(logger, html);
    }

    private static string LoadHtmlFromFile(ILogger<FakeHtmlDownloader> logger)
    {
        string path = @"d:\NetCatHook_TestData\weather_page.html";

        if (!File.Exists(path))
        {
            logger.LogError($"File not exists: {path}");
            return string.Empty;
        }

        return File.ReadAllText(path);
    }

}
