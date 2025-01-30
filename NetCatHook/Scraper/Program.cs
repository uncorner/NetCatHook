using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCatHook.Scraper.App.HostedServices;
using NetCatHook.Scraper.Infrastructure;

//namespace NetCatHook.Scraper;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddCustomServices(builder.Configuration);
builder.Services.AddHostedService<MessengerHostedService>();

var host = builder.Build();
host.Run();

//var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//builder.Services.AddControllers();
//builder.Services.AddCustomServices(builder.Configuration);

//var app = builder.Build();

// Configure the HTTP request pipeline.

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

//app.Run();