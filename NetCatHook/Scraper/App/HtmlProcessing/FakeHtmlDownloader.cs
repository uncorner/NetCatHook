namespace NetCatHook.Scraper.App.HtmlProcessing;

class FakeHtmlDownloader : IHtmlSource
{
    private readonly ILogger<FakeHtmlDownloader> logger;
    private readonly string resultHtml;

    public FakeHtmlDownloader(ILogger<FakeHtmlDownloader> logger, string resultHtml)
    {
        this.logger = logger;
        this.resultHtml = resultHtml;
    }

    public int SlowMo { get => default; set => _ = default(int); }

    public Task<string> GetHtmlDataAsync(string url)
    {
        logger.LogInformation($"CALL FAKE HTML DOWNLOADER");
        return Task.FromResult(resultHtml);
    }

    public static FakeHtmlDownloader CreateEmptyHtml(IServiceProvider service)
    {
        var logger = service.GetRequiredService<ILogger<FakeHtmlDownloader>>();
        return new FakeHtmlDownloader(logger, string.Empty);
    }

}
