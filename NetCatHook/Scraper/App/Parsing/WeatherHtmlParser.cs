using System.Net;
using System.Text.RegularExpressions;

namespace NetCatHook.Scraper.App.Parsing;

class WeatherHtmlParser
{
    private readonly string html;

    public WeatherHtmlParser(string html)
    {
        this.html = html;
    }

    public (bool success, int temp) TryParse()
    {
        Regex regex = new Regex(@",""temperatureAir"":\[([-]?[0-9]+)\]",
            RegexOptions.Multiline | RegexOptions.CultureInvariant);

        // TODO: async
        MatchCollection matches = regex.Matches(html);

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
