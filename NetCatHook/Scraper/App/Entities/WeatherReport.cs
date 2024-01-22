using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NetCatHook.Scraper.App.Entities;

class WeatherReport
{
    public int Id { get; set; }

    /// <summary>
    /// UTC
    /// </summary>
    public DateTime CreatedAt { get; set; }

    [NotMapped]
    public DateTime CreatedAtLocal
    {
        get => CreatedAt.ToLocalTime();
        set => CreatedAt = value.ToUniversalTime();
    }

    public int? TemperatureAir { get; set; }

    [MaxLength(150)]
    public string? Description { get; set; }

    public int? Humidity { get; set; }

    public int? Pressure { get; set; }

    public int? WindDirection { get; set; }

    public int? WindSpeed { get; set; }

    public int? WindGust { get; set; }

    public void SetWeatherData(WeatherData data)
    {
        TemperatureAir = data.TemperatureAir;
        Description = data.Description;
        Humidity = data.Humidity;
        Pressure = data.Pressure;
        WindDirection = data.WindDirection;
        WindSpeed = data.WindSpeed;
        WindGust = data.WindGust;
    }

    internal static WeatherReport CreateExpired()
    {
        return new WeatherReport { CreatedAt = DateTime.MinValue };
    }


}
