using NetCatHook.Scraper.App;
using NetCatHook.Scraper.Infrastructure;

namespace NetCatHook.Scraper;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddCustomServices();

        var app = builder.Build();

        // Configure the HTTP request pipeline.

        app.UseHttpsRedirection();

        //app.UseAuthorization();

        app.MapControllers();

        await using var scheduler = app.Services.GetRequiredService<SimpleScheduler>();
        scheduler.Start(TimeSpan.FromHours(2));

        app.Run();

    }
}