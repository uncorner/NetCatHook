using System.Net;

namespace NetCatHook.Scraper.Infrastructure;

class HtmlDownloader
{
    public async Task<string> CallUrl(string fullUrl)
    {
        HttpClient client = new HttpClient();
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;
        client.DefaultRequestHeaders.Accept.Clear();

        var response = client.GetStringAsync(fullUrl);
        return await response;
    }
}
