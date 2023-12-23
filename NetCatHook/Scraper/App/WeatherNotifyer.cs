namespace NetCatHook.Scraper.App;

class WeatherNotifyer
{
    public delegate void WeatherMessageHandler(string message);

    public event WeatherMessageHandler? Event;

    public void SendMessage(string text)
    {
        Event?.Invoke(text);
    }

}
