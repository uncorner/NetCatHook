using Microsoft.EntityFrameworkCore;
using NetCatHook.Scraper.App.Repository;

namespace NetCatHook.Scraper.Infrastructure.Repository;

class UnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly IDbContextFactory<ApplicationDbContext> dbContextFactory;

    public UnitOfWorkFactory(IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        this.dbContextFactory = dbContextFactory;
    }

    public IUnitOfWork CreateUnitOfWork() => new UnitOfWork(dbContextFactory.CreateDbContext());

}
