using NetCatHook.Scraper.Domain.Entities;

namespace NetCatHook.Scraper.Application.Repository;

interface IWeatherReportRepository
{
    Task Add(WeatherReport weatherReport);

    Task<WeatherReport?> GetLast();

}
