using NetCatHook.Scraper.App.Parsing;
using NetCatHook.Scraper.Infrastructure;
using Xunit.Abstractions;

namespace NetCatHook.ScraperTests.App.Parsing;

public class WeatherHtmlParserTest
{
    public const string TargetUrl = "https://www.gismeteo.ru/weather-ryazan-4394/now/";
    private readonly ITestOutputHelper output;

    public WeatherHtmlParserTest(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public async Task TestTryParse()
    {
        var downloader = new HtmlDownloader();
        var html = await downloader.GetHtmlDataAsync(TargetUrl);

        var parser = new WeatherHtmlParser(html);
        var result = await parser.TryParseAsync();

        Assert.True(result.success);
        Assert.True(result.temp > -40 && result.temp < 40);
        output.WriteLine(result.ToString());
    }

}
