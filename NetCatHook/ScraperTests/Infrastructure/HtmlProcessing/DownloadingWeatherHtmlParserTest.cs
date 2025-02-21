using NetCatHook.Scraper.Infrastructure.WeatherService;
using Xunit.Abstractions;

namespace NetCatHook.ScraperTests.Infrastructure.HtmlProcessing;

public class DownloadingWeatherHtmlParserTest(ITestOutputHelper output)
{
    private const string TargetUrl = "https://www.gi" + "smeteo.ru/weather-moscow-4368/now/";

    [Fact]
    public async Task TestDownloadAndParse()
    {
        var downloader = new BrowserHtmlDownloader();
        var html = await downloader.GetHtmlDataAsync(TargetUrl);

        var parser = new WeatherHtmlParser();
        var result = await parser.TryParseAsync(html);

        Assert.True(result.Processed);
        output.WriteLine(result.ToString());
    }

}
