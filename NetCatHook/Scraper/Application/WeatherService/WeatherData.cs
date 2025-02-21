namespace NetCatHook.Scraper.Application.WeatherService;

record WeatherData(bool Processed,
    int? TemperatureAir = null,
    string? Description = null, int? Humidity = null, int? Pressure = null,
    int? WindDirection = null, int? WindSpeed = null, int? WindGust = null,
    string? City = null, string? InCity = null);
