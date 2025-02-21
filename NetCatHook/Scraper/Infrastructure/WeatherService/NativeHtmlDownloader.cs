using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetCatHook.Scraper.Application;
using NetCatHook.Scraper.Application.WeatherService;

namespace NetCatHook.Scraper.Infrastructure.WeatherService;

class NativeHtmlDownloader : IHtmlSource
{
    private readonly IConfiguration config;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILogger<NativeHtmlDownloader> logger;
    private readonly string[] userAgentList;

    public int SlowMo { get => default; set => _ = default(int); }

    static NativeHtmlDownloader()
    {
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;
    }

    public NativeHtmlDownloader(IConfiguration config,
        IHttpClientFactory httpClientFactory,
        ILogger<NativeHtmlDownloader> logger)
    {
        this.config = config;
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;
        userAgentList = this.config.GetParsingUserAgentList().ToArray();
    }

    public async Task<string> GetHtmlDataAsync(string url)
    {
        var httpClient = httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Accept.Clear();
        var userAgent = GetRandomUserAgent();
        logger.LogInformation($"Set random User-Agent [{userAgent.Item1}], '{userAgent.Item2}'");
        httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent.Item2);

        var response = httpClient.GetStringAsync(url);
        return await response;
    }

    private (int,string) GetRandomUserAgent()
    {
        var random = new Random();
        int randomIndex = random.Next(userAgentList.Length);
        return (randomIndex, userAgentList[randomIndex]);
    }

}
