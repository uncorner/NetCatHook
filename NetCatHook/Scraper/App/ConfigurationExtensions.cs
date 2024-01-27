﻿namespace NetCatHook.Scraper.App;

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

    public static bool GetFakeHtmlDownloaderEnabled(this IConfiguration config)
    {
        return config.GetValue<bool>("FakeHtmlDownloaderEnabled");
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
