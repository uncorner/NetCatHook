using System.Text.RegularExpressions;

namespace NetCatHook.Scraper.App.Parsing;

class WeatherHtmlParser
{
    private readonly string html;

    public WeatherHtmlParser(string html)
    {
        this.html = html;
    }

    public async Task<(bool success, int temp)> TryParseAsync()
    {
        return await Task.Run(DoTryParse);
    }

    private (bool success, int temp) DoTryParse()
    {
        var regex = new Regex(@",""temperatureAir"":\[([-]?[0-9]+)\]",
            RegexOptions.Multiline | RegexOptions.CultureInvariant);

        var matches = regex.Matches(html);

        var match = matches.FirstOrDefault();
        if (match is not null)
        {
            var val = match.Groups[1].Value;
            var success = int.TryParse(val, out int temp);

            return (success, temp);
        }

        return (false, default(int));
    }
    

}
