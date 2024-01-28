using NetCatHook.Scraper.App;
using NetCatHook.Scraper.App.HtmlProcessing;
using System.Net;

namespace NetCatHook.Scraper.Infrastructure.HtmlProcessing;

class NativeHtmlDownloader : IHtmlSource
{
    private readonly IConfiguration config;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly string[] userAgentList;

    public int SlowMo { get => default; set => _ = default(int); }

    static NativeHtmlDownloader()
    {
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;
    }

    public NativeHtmlDownloader(IConfiguration config,
        IHttpClientFactory httpClientFactory)
    {
        this.config = config;
        this.httpClientFactory = httpClientFactory;
        userAgentList = this.config.GetParsingUserAgentList().ToArray();
    }

    public async Task<string> GetHtmlDataAsync(string url)
    {
        var httpClient = httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Add("User-Agent", GetRandomUserAgent());

        var response = httpClient.GetStringAsync(url);
        return await response;
    }

    private string GetRandomUserAgent()
    {
        var random = new Random();
        int randomIndex = random.Next(userAgentList.Length);
        return userAgentList[randomIndex];
    }

}
