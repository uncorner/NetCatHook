using Microsoft.EntityFrameworkCore;
using NetCatHook.Scraper.Application.Repository;

namespace NetCatHook.Scraper.Infrastructure.Repository;

class DbUnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly IDbContextFactory<ApplicationDbContext> dbContextFactory;

    public DbUnitOfWorkFactory(IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        this.dbContextFactory = dbContextFactory;
    }

    public IUnitOfWork CreateUnitOfWork() => new DbUnitOfWork(dbContextFactory.CreateDbContext());

}
