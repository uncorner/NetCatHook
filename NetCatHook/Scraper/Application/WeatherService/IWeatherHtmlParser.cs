namespace NetCatHook.Scraper.Application.WeatherService;

interface IWeatherHtmlParser
{
    public Task<WeatherData> TryParseAsync(string html);
}
