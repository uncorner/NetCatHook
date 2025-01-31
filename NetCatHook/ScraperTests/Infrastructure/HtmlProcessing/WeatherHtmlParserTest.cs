using NetCatHook.Scraper.Infrastructure.HtmlProcessing;
using Xunit.Abstractions;

namespace NetCatHook.ScraperTests.Infrastructure.HtmlProcessing;

public partial class WeatherHtmlParserTest(ITestOutputHelper output)
{
    [Fact]
    public async Task TestTryParse()
    {
        var parser = new WeatherHtmlParser();
        var result = await parser.TryParseAsync(HtmlPart);

        Assert.True(result.Processed);
        Assert.NotNull(result.Description);
        Assert.Equal("Пасмурно", result.Description);
        
        Assert.NotNull(result.Humidity);
        Assert.Equal(86, result.Humidity);
        
        Assert.NotNull(result.TemperatureAir);
        Assert.Equal(3, result.TemperatureAir);

        Assert.NotNull(result.WindDirection);
        Assert.Equal(277, result.WindDirection);

        Assert.NotNull(result.WindSpeed);
        Assert.Equal(1, result.WindSpeed);

        Assert.NotNull(result.WindGust);
        Assert.Equal(3, result.WindGust);
        
        Assert.NotNull(result.City);
        Assert.Equal("Москва", result.City);
        Assert.NotNull(result.InCity);
        Assert.Equal("в Москве", result.InCity);

        output.WriteLine(result.ToString());
    }

    [Fact]
    public async Task TestTryParseWhenNotAllValues()
    {
        var parser = new WeatherHtmlParser();
        var result = await parser.TryParseAsync(HtmlPart2);

        Assert.True(result.Processed);
        Assert.NotNull(result.TemperatureAir);
        Assert.NotNull(result.Description);
        Assert.NotNull(result.Humidity);
        Assert.NotNull(result.WindDirection);
        Assert.NotNull(result.WindSpeed);
        Assert.Null(result.WindGust);
        Assert.NotNull(result.City);
        Assert.NotNull(result.InCity);
        
        output.WriteLine(result.ToString());
    }

}
