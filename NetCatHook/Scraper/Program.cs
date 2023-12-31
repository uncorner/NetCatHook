﻿using NetCatHook.Scraper.Infrastructure;

namespace NetCatHook.Scraper;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddCustomServices(builder.Configuration);
        
        var app = builder.Build();

        // Configure the HTTP request pipeline.

        app.UseHttpsRedirection();

        //app.UseAuthorization();

        app.MapControllers();
        
        app.Run();

    }
}