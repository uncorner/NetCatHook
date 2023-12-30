using NetCatHook.Scraper.App;
using NetCatHook.Scraper.App.Parsing;
using Xunit.Abstractions;

namespace NetCatHook.ScraperTests.App;

public class WeatherEvaluatorTest
{
    private readonly ITestOutputHelper output;

    public WeatherEvaluatorTest(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void TestEvaluate()
    {
        var data = new WeatherData(true, TemperatureAir: -10);
        var result = WeatherEvaluator.Evaluate(data);

        Assert.True(result.Processed);
        Assert.False(string.IsNullOrWhiteSpace(result.TextMessage));
        output.WriteLine(result.ToString());
    }

}
