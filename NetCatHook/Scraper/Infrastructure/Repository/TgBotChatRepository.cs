using Microsoft.EntityFrameworkCore;
using NetCatHook.Scraper.App.Entities;
using NetCatHook.Scraper.App.Repository;

namespace NetCatHook.Scraper.Infrastructure.Repository;

internal class TgBotChatRepository : ITgBotChatRepository
{
    private readonly ApplicationDbContext dbContext;

    public TgBotChatRepository(ApplicationDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task AddAsync(TgBotChat product)
    {
        await dbContext.TgBotChats.AddAsync(product);
    }

    //public async Task AddRangeAsync(IEnumerable<TgBotChat> products)
    //{
    //    await dbContext.AddRangeAsync(products);
    //}

    public async Task<IEnumerable<TgBotChat>> GetAllAsync() =>
        await dbContext.TgBotChats.OrderBy(i => i.Id).ToArrayAsync();

    public async Task<TgBotChat?> GetByChatId(long chatId)
    {
        return await dbContext.TgBotChats.FirstOrDefaultAsync(i => i.ChatId == chatId);
    }

    //public async Task<IEnumerable<TgBotChat>> GetByIdsAsync(IEnumerable<int> ids)
    //{
    //    return await dbContext.TgBotChats.Where(i => ids.Contains(i.Id)).ToArrayAsync()
    //        ?? Array.Empty<TgBotChat>();
    //}

    //public async Task<IEnumerable<int>> CheckProductsExistAsync(IEnumerable<int> ids)
    //{
    //    return await dbContext.TgBotChats.Where(i => ids.Contains(i.Id))
    //        .Select(i => i.Id).ToArrayAsync();
    //}

    //public async Task BatchRemoveAsync(IEnumerable<int> ids)
    //{
    //    await dbContext.TgBotChats.Where(i => ids.Contains(i.Id)).ExecuteDeleteAsync();
    //}

}
