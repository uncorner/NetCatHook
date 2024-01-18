using Microsoft.EntityFrameworkCore;
using NetCatHook.Scraper.App;
using NetCatHook.Scraper.App.Entities;

namespace NetCatHook.Scraper.Infrastructure;

public class ApplicationDbContext : DbContext
{
    private readonly IConfiguration configuration;

    internal DbSet<TgBotChat> TgBotChats { get; set; } = null!;
    internal DbSet<WeatherReport> WeatherReports { get; set; } = null!;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
        IConfiguration configuration)
        : base(options)
    {
        this.configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(configuration.GetDbConnectionString());
    }

}
