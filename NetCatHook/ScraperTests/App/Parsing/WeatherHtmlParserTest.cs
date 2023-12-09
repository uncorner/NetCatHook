using NetCatHook.Scraper.App.Parsing;
using NetCatHook.Scraper.Infrastructure;

namespace NetCatHook.ScraperTests.App.Parsing;

public class WeatherHtmlParserTest
{
    public const string TargetUrl = "https://www.gismeteo.ru/weather-ryazan-4394/now/";

    [Fact]
    public async Task TestParse()
    {
        var downloader = new HtmlDownloader();
        var html = await downloader.CallUrl(TargetUrl);

        var parser = new WeatherHtmlParser(html);
        var value = parser.Parse();

        Assert.True(value > -40 && value < 40);
    }


}
