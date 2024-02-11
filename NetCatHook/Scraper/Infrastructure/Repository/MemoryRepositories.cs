using NetCatHook.Scraper.App.Entities;
using NetCatHook.Scraper.App.Repository;
using System.Collections.Concurrent;

namespace NetCatHook.Scraper.Infrastructure.Repository;

class MemoryUnitOfWorkFactory : IUnitOfWorkFactory
{
    public IUnitOfWork CreateUnitOfWork()
    {
        return new MemoryUnitOfWork();
    }
}

file class MemoryUnitOfWork : IUnitOfWork
{
    public ITgBotChatRepository CreateTgBotChatRepository()
    {
        return new MemoryTgBotChatRepository();
    }

    public IWeatherReportRepository CreateWeatherReportRepository()
    {
        return new MemoryWeatherReportRepository();
    }

    public int SaveChanges()
    {
        return default;
    }

    public Task<int> SaveChangesAsync()
    {
        return Task.FromResult(default(int));
    }

    public void Dispose()
    {
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}

file class MemoryTgBotChatRepository : ITgBotChatRepository
{
    private readonly ConcurrentDictionary<int, TgBotChat> chatDict = new();

    public Task Add(TgBotChat chat)
    {
        var maxId = chatDict.Keys.Any() ? chatDict.Keys.Max() : 1;
        chat.Id = maxId++;

        if (!chatDict.TryAdd(chat.Id, chat))
        {
            throw new Exception(nameof(Add));
        }
        return Task.CompletedTask;
    }

    public Task<IEnumerable<TgBotChat>> GetAllEnabled()
    {
        var items = chatDict.Values.Where(e => e.IsEnabled).AsEnumerable();
        return Task.FromResult(items);
    }

    public Task<TgBotChat?> GetByChatId(long chatId)
    {
        var chat = chatDict.Values.Where(e => e.ChatId == chatId).FirstOrDefault();
        return Task.FromResult(chat);
    }
}

file class MemoryWeatherReportRepository : IWeatherReportRepository
{
    private readonly ConcurrentDictionary<int, WeatherReport> reportDict = new();

    public Task Add(WeatherReport weatherReport)
    {
        var maxId = reportDict.Keys.Any() ? reportDict.Keys.Max() : 1;
        weatherReport.Id = maxId++;

        if (!reportDict.TryAdd(weatherReport.Id, weatherReport))
        {
            throw new Exception(nameof(Add));
        }
        return Task.CompletedTask;
    }

    public Task<WeatherReport?> GetLast()
    {
        var report = reportDict.Values.OrderByDescending(e => e.CreatedAt)
            .FirstOrDefault();
        return Task.FromResult(report);
    }
}



