using NetCatHook.Scraper.App.Parsing;
using NetCatHook.Scraper.Infrastructure;
using Xunit.Abstractions;

namespace NetCatHook.ScraperTests.App.Parsing
{
    public class DownloadingWeatherHtmlParserTest
    {
        public const string TargetUrl = "https://www.gismeteo.ru/weather-ryazan-4394/now/";
        private readonly ITestOutputHelper output;

        public DownloadingWeatherHtmlParserTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public async Task TestDownloadAndParse()
        {
            var downloader = new HtmlDownloader();
            var html = await downloader.GetHtmlDataAsync(TargetUrl);

            var parser = new WeatherHtmlParser();
            var result = await parser.TryParseAsync(html);

            Assert.True(result.IsSuccess);
            output.WriteLine(result.ToString());
        }

    }
}
