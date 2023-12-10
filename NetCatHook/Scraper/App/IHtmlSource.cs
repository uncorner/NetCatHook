namespace NetCatHook.Scraper.App;

interface IHtmlSource
{
    public Task<string> GetHtmlDataAsync(string url);

}
