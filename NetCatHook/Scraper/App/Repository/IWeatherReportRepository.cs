using NetCatHook.Scraper.Domain.Entities;

namespace NetCatHook.Scraper.App.Repository;

interface IWeatherReportRepository
{
    Task Add(WeatherReport weatherReport);

    Task<WeatherReport?> GetLast();

}
