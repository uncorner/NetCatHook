namespace NetCatHook.Scraper.Application.WeatherService;

interface IHtmlSource
{
    public Task<string> GetHtmlDataAsync(string url);

    public int SlowMo { get; set; }

}
