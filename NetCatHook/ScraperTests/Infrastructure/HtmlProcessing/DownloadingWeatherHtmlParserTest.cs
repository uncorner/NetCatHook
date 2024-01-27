using NetCatHook.Scraper.Infrastructure.HtmlProcessing;
using Xunit.Abstractions;

namespace NetCatHook.ScraperTests.Infrastructure.HtmlProcessing;

public class DownloadingWeatherHtmlParserTest
{
    public const string TargetUrl = "https://www.gismeteo.ru/weather-moscow-4368/now/";
    private readonly ITestOutputHelper output;

    public DownloadingWeatherHtmlParserTest(ITestOutputHelper output)
    {
        this.output = output;
    }

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
