using Microsoft.EntityFrameworkCore;
using NetCatHook.Scraper.App.Entities;

namespace NetCatHook.Scraper.Infrastructure;

public class ApplicationDbContext : DbContext
{
    internal DbSet<TgBotChat> TgBotChats { get; set; } = null!;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

}

