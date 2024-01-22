using NetCatHook.Scraper.App.Entities;

namespace NetCatHook.Scraper.App;

class WeatherNotifyer
{
    public delegate void WeatherMessageHandler(string? message, WeatherReport weatherReport);

    public event WeatherMessageHandler? Event;

    public void SendData(string? message, WeatherReport weatherReport)
    {
        Event?.Invoke(message, weatherReport);
    }

}
