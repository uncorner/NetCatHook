﻿using NetCatHook.Scraper.Application.Repository;

namespace NetCatHook.Scraper.Infrastructure.Repository;

sealed class DbUnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext dbContext;

    public DbUnitOfWork(ApplicationDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    #region Repositories

    public ISubjectChatRepository CreateSubjectChatRepository() =>
        new DbSubjectChatRepository(dbContext);

    public IWeatherReportRepository CreateWeatherReportRepository() =>
        new DbWeatherReportRepository(dbContext);

    #endregion

    public int SaveChanges()
    {
        return dbContext.SaveChanges();
    }

    public async Task<int> SaveChangesAsync()
    {
        return await dbContext.SaveChangesAsync();
    }

    #region IDisposable, IAsyncDisposable
    public void Dispose()
    {
        dbContext.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await dbContext.DisposeAsync();
    }
    #endregion

}

