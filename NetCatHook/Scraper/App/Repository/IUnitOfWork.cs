namespace NetCatHook.Scraper.App.Repository;

interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    ITgBotChatRepository CreateTgBotChatRepository();
    int SaveChanges();
    Task<int> SaveChangesAsync();
}

