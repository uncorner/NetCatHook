using Microsoft.Extensions.Configuration;

namespace NetCatHook.Scraper.Application;

static class ConfigurationExtensions
{
    public static string GetTgBotSecureToken(this IConfiguration config)
    {
        return config["TgBotSecureToken"]!;
    }

    public static int GetParsingSchedulerTimeoutInMinutes(this IConfiguration config)
    {
        return config.GetValue<int>("ParsingSchedulerTimeoutInMinutes");
    }

    public static string GetDbConnectionString(this IConfiguration config)
    {
        return config.GetConnectionString("Default")!;
    }

    public static string GetWeatherParsingUrl(this IConfiguration config)
    {
        return config["WeatherParsingUrl"]!;
    }

    public static HtmlDownloaderTypes GetHtmlDownloaderType(this IConfiguration config)
    {
        var value = config.GetValue<string>("HtmlDownloaderType");
        Enum.TryParse<HtmlDownloaderTypes>(value, out var downloaderType);
        return downloaderType;
    }

    public static bool GetLoggerNotificationsEnabled(this IConfiguration config)
    {
        return config.GetValue<bool>("LoggerNotificationsEnabled");
    }

    public static IEnumerable<string> GetParsingUserAgentList(this IConfiguration config)
    {
        return config.GetSection("ParsingUserAgentList").Get<IEnumerable<string>>()!;
    }

}
