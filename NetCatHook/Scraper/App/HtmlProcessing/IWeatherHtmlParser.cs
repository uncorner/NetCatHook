namespace NetCatHook.Scraper.App.HtmlProcessing;

interface IWeatherHtmlParser
{
    public Task<WeatherData> TryParseAsync(string html);
}
