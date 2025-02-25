namespace NetCatHook.Scraper.Application.Repository;

interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    #region Repositories
    ISubjectChatRepository CreateSubjectChatRepository();
    IWeatherReportRepository CreateWeatherReportRepository();
    #endregion
    
    int SaveChanges();
    Task<int> SaveChangesAsync();
}

