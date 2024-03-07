using Microsoft.EntityFrameworkCore;
using NetCatHook.Scraper.App.Entities;
using NetCatHook.Scraper.App.Repository;

namespace NetCatHook.Scraper.Infrastructure.Repository;

class DbWeatherReportRepository : IWeatherReportRepository
{
    private readonly ApplicationDbContext dbContext;

    public DbWeatherReportRepository(ApplicationDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task Add(WeatherReport weatherReport)
    {
        await dbContext.WeatherReports.AddAsync(weatherReport);
    }

    public async Task<WeatherReport?> GetLast()
    {
        return await dbContext.WeatherReports.OrderByDescending(e => e.CreatedAt)
            .FirstOrDefaultAsync();
    }

}
