using NetCatHook.Scraper.App;
using System.Net;

namespace NetCatHook.Scraper.Infrastructure;

class HtmlDownloader : IHtmlSource
{
    public async Task<string> GetHtmlDataAsync(string url)
    {
        var client = new HttpClient();
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;
        client.DefaultRequestHeaders.Accept.Clear();

        var response = client.GetStringAsync(url);
        return await response;
    }
}
