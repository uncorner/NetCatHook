using NetCatHook.Scraper.App.HtmlProcessing;
using PuppeteerSharp;

namespace NetCatHook.Scraper.Infrastructure.HtmlProcessing;

class BrowserHtmlDownloader : IHtmlSource
{
    public int SlowMo { get; set; } = 10;

    public async Task<string> GetHtmlDataAsync(string url)
    {
        using var browserFetcher = new BrowserFetcher();
        await browserFetcher.DownloadAsync();

        var options = new LaunchOptions()
        {
 #if OS_WINDOWS
             ExecutablePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe",
 #else
             ExecutablePath = @"/usr/bin/google-chrome",
 #endif
            Headless = true, // browser invisible mode
            SlowMo = SlowMo,
            Timeout = 60000*3,
        };
        await using var browser = await Puppeteer.LaunchAsync(options);
        await using var page = await browser.NewPageAsync();

        await page.GoToAsync(url, new NavigationOptions
        {
            // WaitUntil = new[] { WaitUntilNavigation.Load },
            // Timeout = 0
            WaitUntil = [ WaitUntilNavigation.DOMContentLoaded ],
            Timeout = 60000*5
        });
        var content = await page.GetContentAsync();

        return content ?? string.Empty;
    }

}
