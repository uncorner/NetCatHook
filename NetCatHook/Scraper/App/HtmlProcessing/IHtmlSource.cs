namespace NetCatHook.Scraper.App.HtmlProcessing;

interface IHtmlSource
{
    public Task<string> GetHtmlDataAsync(string url);

    public int SlowMo { get; set; }

}
