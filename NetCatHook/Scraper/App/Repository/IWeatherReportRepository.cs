using NetCatHook.Scraper.App.Entities;

namespace NetCatHook.Scraper.App.Repository;

interface IWeatherReportRepository
{
    Task Add(WeatherReport weatherReport);

    Task<WeatherReport?> GetLast();

}
