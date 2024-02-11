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

    public async Task Add(TgBotChat product)
    {
        await dbContext.TgBotChats.AddAsync(product);
    }

    public async Task<IEnumerable<TgBotChat>> GetAllEnabled() =>
        await dbContext.TgBotChats.Where(i => i.IsEnabled)
        .OrderBy(i => i.Id).ToArrayAsync();

    public async Task<TgBotChat?> GetByChatId(long chatId)
    {
        return await dbContext.TgBotChats.FirstOrDefaultAsync(i => i.ChatId == chatId);
    }

}
