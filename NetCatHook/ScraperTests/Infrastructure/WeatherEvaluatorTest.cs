using NetCatHook.Scraper.Application;
using Xunit.Abstractions;

namespace NetCatHook.ScraperTests.Infrastructure;

public class WeatherEvaluatorTest(ITestOutputHelper output)
{
    [Fact]
    public void TestEvaluate()
    {
        var data = new WeatherData(true, TemperatureAir: -10, Pressure: 755,
            WindSpeed: 7, WindGust: 10, Humidity: 85);
        var result = WeatherEvaluator.Evaluate(data);

        Assert.True(result.Processed);
        Assert.False(string.IsNullOrWhiteSpace(result.TextMessage));
        output.WriteLine(result.ToString());
    }

    [Fact]
    public void TestEvaluateNotProcessed()
    {
        var data = new WeatherData(true, TemperatureAir: 20, Pressure: 745,
            WindSpeed: 2, WindGust: null, Humidity: 50);
        var result = WeatherEvaluator.Evaluate(data);

        Assert.False(result.Processed);
        Assert.True(string.IsNullOrWhiteSpace(result.TextMessage));
    }

}