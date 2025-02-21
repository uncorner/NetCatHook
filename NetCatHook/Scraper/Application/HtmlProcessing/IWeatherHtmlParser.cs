namespace NetCatHook.Scraper.Application.HtmlProcessing;

interface IWeatherHtmlParser
{
    public Task<WeatherData> TryParseAsync(string html);
}
