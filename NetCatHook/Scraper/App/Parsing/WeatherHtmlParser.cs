using System.Text.RegularExpressions;

namespace NetCatHook.Scraper.App.Parsing;

class WeatherHtmlParser
{

    public async Task<(bool success, int temp)> TryParseAsync(string html)
    {
        return await Task.Run(() => TryParse(html));
    }

    public (bool success, int temp) TryParse(string html)
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
