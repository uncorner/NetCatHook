using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCatHook.Scraper.Infrastructure;
using NetCatHook.Scraper.Presentation;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddCustomServices(builder.Configuration);
builder.Services.AddHostedService<MessengerHostedService>();

var host = builder.Build();
host.Run();
