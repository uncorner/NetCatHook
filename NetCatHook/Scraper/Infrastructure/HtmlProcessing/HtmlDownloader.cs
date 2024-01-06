using NetCatHook.Scraper.App.HtmlProcessing;
using System.Net;

namespace NetCatHook.Scraper.Infrastructure.HtmlProcessing;

class HtmlDownloader : IHtmlSource
{
    private static readonly List<string> userAgents = new List<string>
        {
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36",
            "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36 OPR/105.0.0.0 (Edition Yx 05)",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:120.0) Gecko/20100101 Firefox/120.0",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/118.0.5993.771 YaBrowser/23.11.2.771 Yowser/2.5 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36 Edg/120.0.0.0"
        };

    public int SlowMo { get => default; set => _ = default(int); }

    static HtmlDownloader()
    {
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;
    }

    public async Task<string> GetHtmlDataAsync(string url)
    {
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Add("User-Agent", GetRandomUserAgent());

        var response = httpClient.GetStringAsync(url);
        return await response;
    }

    private static string GetRandomUserAgent()
    {
        var random = new Random();
        int randomIndex = random.Next(userAgents.Count);
        return userAgents[randomIndex];
    }

}
