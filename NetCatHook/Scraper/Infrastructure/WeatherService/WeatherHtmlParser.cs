using System.Text.RegularExpressions;
using NetCatHook.Scraper.Application;
using NetCatHook.Scraper.Application.WeatherService;

namespace NetCatHook.Scraper.Infrastructure.WeatherService;

class WeatherHtmlParser : IWeatherHtmlParser
{
    private const string Pattern =
        @"window\.M\.state = \{"
        + $@".+\{{""city"":\{{""{Grp.name}"":(?<{Grp.name}>[^,]+),""{Grp.nameP}"":(?<{Grp.nameP}>[^,]+),""nameR"""
        + $@".+""{Grp.description}"":\[(?<{Grp.description}>[^\]]+)\]"
        + $@".+""{Grp.humidity}"":\[(?<{Grp.humidity}>[^\]]+)\]"
        + $@".+""{Grp.pressure}"":\[(?<{Grp.pressure}>[^\]]+)\]"
        + $@".+""{Grp.temperatureAir}"":\[(?<{Grp.temperatureAir}>[^\]]+)\]"
        + $@".+""{Grp.windDirection}"":\[(?<{Grp.windDirection}>[^\]]+)\]"
        + $@".+""{Grp.windSpeed}"":\[(?<{Grp.windSpeed}>[^\]]+)\]"
        + $@".+""{Grp.windGust}"":\[(?<{Grp.windGust}>[^\]]+)\]";
    
    private static class Grp
    {
        // ReSharper disable once InconsistentNaming
        public const string description = nameof(description);
        // ReSharper disable once InconsistentNaming
        public const string temperatureAir = nameof(temperatureAir);
        // ReSharper disable once InconsistentNaming
        public const string humidity = nameof(humidity);
        // ReSharper disable once InconsistentNaming
        public const string pressure = nameof(pressure);
        // ReSharper disable once InconsistentNaming
        public const string windDirection = nameof(windDirection);
        // ReSharper disable once InconsistentNaming
        public const string windSpeed = nameof(windSpeed);
        // ReSharper disable once InconsistentNaming
        public const string windGust = nameof(windGust);
        // ReSharper disable once InconsistentNaming
        public const string name = nameof(name);
        // ReSharper disable once InconsistentNaming
        public const string nameP = nameof(nameP);
    }

    public async Task<WeatherData> TryParseAsync(string html)
    {
        return await Task.Run(() => TryParse(html));
    }

    private static WeatherData TryParse(string html)
    {
        var regex = new Regex(Pattern, RegexOptions.Multiline
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

            strValue = match.Groups[Grp.name].Value;
            var city = ParseString(strValue);
            strValue = match.Groups[Grp.nameP].Value;
            var inCity = ParseString(strValue);

            return new WeatherData(true, temperatureAir, description,
                humidity, pressure, windDirection, windSpeed,
                windGust, city, inCity);
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
