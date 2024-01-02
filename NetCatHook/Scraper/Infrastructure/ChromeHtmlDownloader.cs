﻿using NetCatHook.Scraper.App;
using PuppeteerSharp;

namespace NetCatHook.Scraper.Infrastructure;

public class ChromeHtmlDownloader : IHtmlSource
{

    public async Task<string> GetHtmlDataAsync(string url)
    {
        using var browserFetcher = new BrowserFetcher();
        await browserFetcher.DownloadAsync();

        var options = new LaunchOptions()
        {
            ExecutablePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe",
            Headless = true,
            SlowMo = 10
        };
        await using var browser = await Puppeteer.LaunchAsync(options);
        await using var page = await browser.NewPageAsync();
        await page.GoToAsync(url);
        var content = await page.GetContentAsync();

        return content ?? string.Empty;
    }

}
