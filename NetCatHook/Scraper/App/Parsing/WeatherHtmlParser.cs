using System.Text.RegularExpressions;

namespace NetCatHook.Scraper.App.Parsing;

class WeatherHtmlParser
{
    private const string pattern =
        @"M\.state\.weather\.cw = \{"
        + $@".+""{Grp.description}"":\[(?<{Grp.description}>[^\]]+)\]"
        + $@".+""{Grp.temperatureAir}"":\[(?<{Grp.temperatureAir}>[^\]]+)\]"
        + $@".+""{Grp.humidity}"":\[(?<{Grp.humidity}>[^\]]+)\]"
        + $@".+""{Grp.pressure}"":\[(?<{Grp.pressure}>[^\]]+)\]"
        + $@".+""{Grp.windDirection}"":\[(?<{Grp.windDirection}>[^\]]+)\]"
        + $@".+""{Grp.windSpeed}"":\[(?<{Grp.windSpeed}>[^\]]+)\]"
        + $@".+""{Grp.windGust}"":\[(?<{Grp.windGust}>[^\]]+)\]"
        + @".*\}";

    private static class Grp
    {
        public const string description = nameof(description);
        public const string temperatureAir = nameof(temperatureAir);
        public const string humidity = nameof(humidity);
        public const string pressure = nameof(pressure);
        public const string windDirection = nameof(windDirection);
        public const string windSpeed = nameof(windSpeed);
        public const string windGust = nameof(windGust);
    }

    public async Task<WeatherData> TryParseAsync(string html)
    {
        return await Task.Run(() => TryParse(html));
    }

    public WeatherData TryParse(string html)
    {
        var regex = new Regex(pattern, RegexOptions.Multiline
            | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        var matches = regex.Matches(html);

        var match = matches.FirstOrDefault();
        if (match is not null)
        {
            var strValue = match.Groups[Grp.temperatureAir].Value;
            var temperatureAir = ParseInt(strValue);

            strValue = match.Groups[Grp.description].Value;
            var description = ParseString(strValue);

            strValue = match.Groups[Grp.humidity].Value;
            var humidity = ParseInt(strValue);

            strValue = match.Groups[Grp.pressure].Value;
            var pressure = ParseInt(strValue);

            strValue = match.Groups[Grp.windDirection].Value;
            var windDirection = ParseInt(strValue);

            strValue = match.Groups[Grp.windSpeed].Value;
            var windSpeed = ParseInt(strValue);

            strValue = match.Groups[Grp.windGust].Value;
            var windGust = ParseInt(strValue);

            return new WeatherData(true, temperatureAir, description,
                humidity, pressure, windDirection, windSpeed, windGust);
        }

        return new WeatherData(false);
    }

    private static int? ParseInt(string? strValue)
    {
        if (IsParsingValueNull(strValue))
        {
            return null;
        }

        if (int.TryParse(strValue, out var intValue))
        {
            return intValue;
        }
        return null;
    }

    private static string? ParseString(string? strValue)
    {
        if (IsParsingValueNull(strValue))
        {
            return null;
        }

        return strValue?.Trim('"');
    }

    private static bool IsParsingValueNull(string? strValue)
    {
        return string.IsNullOrWhiteSpace(strValue)
             || strValue?.ToLowerInvariant() == "null";
    }

}
