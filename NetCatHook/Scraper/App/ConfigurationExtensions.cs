namespace NetCatHook.Scraper.App;

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

}
