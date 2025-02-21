namespace NetCatHook.Scraper.Application.Repository;

interface IUnitOfWorkFactory
{
    public IUnitOfWork CreateUnitOfWork();
}
