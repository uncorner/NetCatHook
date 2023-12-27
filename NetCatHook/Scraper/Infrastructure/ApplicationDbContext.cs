using Microsoft.EntityFrameworkCore;
using NetCatHook.Scraper.App.Entities;

namespace NetCatHook.Scraper.Infrastructure;

public class ApplicationDbContext : DbContext
{
    public DbSet<TgBotChat> TgBotChats { get; set; } = null!;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

}

