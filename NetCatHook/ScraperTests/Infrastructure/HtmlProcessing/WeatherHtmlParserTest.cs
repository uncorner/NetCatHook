using NetCatHook.Scraper.Infrastructure.HtmlProcessing;
using Xunit.Abstractions;

namespace NetCatHook.ScraperTests.Infrastructure.HtmlProcessing;

public partial class WeatherHtmlParserTest
{
    private readonly ITestOutputHelper output;

    public WeatherHtmlParserTest(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public async Task TestTryParse()
    {
        var parser = new WeatherHtmlParser();
        var result = await parser.TryParseAsync(htmlPart);

        Assert.True(result.Processed);
        Assert.NotNull(result.TemperatureAir);
        Assert.Equal(-13, result.TemperatureAir);

        Assert.NotNull(result.Description);
        Assert.Equal("Пасмурно, небольшой  снег", result.Description);

        Assert.NotNull(result.Humidity);
        Assert.Equal(83, result.Humidity);

        Assert.NotNull(result.WindDirection);
        Assert.Equal(90, result.WindDirection);

        Assert.NotNull(result.WindSpeed);
        Assert.Equal(3, result.WindSpeed);

        Assert.NotNull(result.WindGust);
        Assert.Equal(11, result.WindGust);

        output.WriteLine(result.ToString());
    }

    [Fact]
    public async Task TestTryParseWhenNotAllValues()
    {
        var parser = new WeatherHtmlParser();
        var result = await parser.TryParseAsync(htmlPart2);

        Assert.True(result.Processed);
        Assert.NotNull(result.TemperatureAir);
        Assert.NotNull(result.Description);
        Assert.NotNull(result.Humidity);
        Assert.NotNull(result.WindDirection);
        Assert.NotNull(result.WindSpeed);
        Assert.Null(result.WindGust);
        
        output.WriteLine(result.ToString());
    }

}
