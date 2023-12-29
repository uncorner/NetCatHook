using NetCatHook.Scraper.App.Parsing;
using Xunit.Abstractions;

namespace NetCatHook.ScraperTests.App.Parsing;

public class WeatherHtmlParserTest
{
    private const string htmlPart = @" horizontal: []
        },
        info: {}
    }
    try {
        M.state.city = {""source"":""redis"",""id"":4394,""url"":""/weather-ryazan-4394/"",""slug"":""ryazan"",""kind"":""T"",""coordinates"":{""latitude"":54.609501,""longitude"":39.712502},""obsStationId"":11915,""timeZone"":180,""country"":{""id"":156,""url"":""/catalog/russia/"",""code"":""RU""},""district"":{""id"":262,""url"":""/catalog/russia/ryazan-oblast/""},""subdistrict"":{""id"":4774,""url"":""/catalog/russia/ryazan-oblast/ryazan/""},""translations"":{""ru"":{""city"":{""name"":""Рязань"",""nameP"":""в Рязани"",""nameR"":""""},""country"":{""name"":""Россия"",""nameP"":""в России"",""nameR"":""России""},""district"":{""name"":""Рязанская область"",""nameP"":""в Рязанской области"",""nameR"":""Рязанской области""},""subdistrict"":{""name"":""Рязань (городской округ)"",""nameP"":""в Рязани"",""nameR"":""Рязани""}}},""lang"":""ru"",""originUrl"":""/weather-ryazan-4394/"",""dates"":{""local"":""2023-12-09T21:12:34.732Z"",""utc"":""2023-12-09T18:12:34.732Z""}}
        M.state.weather.cw = {""source"":""redis"",""date"":[""2023-12-09T15:00:00.000Z""],""kind"":[""ObsCor""],""iconWeather"":[""n_c3_s1""],""description"":[""Пасмурно, небольшой  снег""],""precipitation"":[0.1],""roadConditionIcon"":[3],""roadConditionCode"":[31],""radiation"":[null],""pollenGrass"":[null],""pollenBirch"":[null],""pollenRagweed"":[null],""snowFall"":[0.1],""snowHeight"":[28.5],""snowIcon"":[""s2""],""temperatureAir"":[-13],""temperatureWater"":[3],""temperatureHeatIndex"":[-19],""iconBg"":[],""visibility"":[null],""humidity"":[83],""pressure"":[760],""windDirection"":[90],""windSpeed"":[3],""windGust"":[11],""stormPrediction"":[],""precipitationType"":[2],""cloudiness"":[100],""dewPoint"":[-16],""importTime"":[""2023-12-09T15:00:00.000Z""],""forecastDate"":[],""type"":""current"",""cityid"":4394,""version"":""v3"",""range"":[0,1]}
    } catch (e) {
        console.error(e)
    }

    M.state.info.articlePollenLink = ";

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

        Assert.True(result.IsSuccess);
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

}
