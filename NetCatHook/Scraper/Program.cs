using NetCatHook.Scraper.App;

namespace NetCatHook.Scraper;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();

        var app = builder.Build();

        // Configure the HTTP request pipeline.

        app.UseHttpsRedirection();

        //app.UseAuthorization();

        app.MapControllers();

        await using var scheduler = new SimpleScheduler(
            app.Services.GetRequiredService<ILogger<SimpleScheduler>>(),
            TimeSpan.FromHours(2) );
        scheduler.Start();

        app.Run();

    }
}