using NetCatHook.Scraper.Domain;
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
        var data = new WeatherData(true, TemperatureAir: -10, Pressure: 755,
            WindSpeed: 7, WindGust: 10);
        var result = WeatherEvaluator.Evaluate(data);

        Assert.True(result.Processed);
        Assert.False(string.IsNullOrWhiteSpace(result.TextMessage));
        output.WriteLine(result.ToString());
    }

    [Fact]
    public void TestEvaluateNotProcessed()
    {
        var data = new WeatherData(true, TemperatureAir: 20, Pressure: 745,
            WindSpeed: 2, WindGust: null);
        var result = WeatherEvaluator.Evaluate(data);

        Assert.False(result.Processed);
        Assert.True(string.IsNullOrWhiteSpace(result.TextMessage));
    }

}