namespace NetCatHook.Scraper.App.Repository;

interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    ITgBotChatRepository CreateTgBotChatRepository();
    IWeatherReportRepository CreateWeatherReportRepository();
    int SaveChanges();
    Task<int> SaveChangesAsync();
}

