using Microsoft.EntityFrameworkCore;
using NetCatHook.Scraper.App.Entities;
using NetCatHook.Scraper.App.Repository;

namespace NetCatHook.Scraper.Infrastructure.Repository;

class TgBotChatRepository : ITgBotChatRepository
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

    public async Task<IEnumerable<TgBotChat>> GetAllAsync() =>
        await dbContext.TgBotChats.OrderBy(i => i.Id).ToArrayAsync();

    public async Task<TgBotChat?> GetByChatId(long chatId)
    {
        return await dbContext.TgBotChats.FirstOrDefaultAsync(i => i.ChatId == chatId);
    }

}
