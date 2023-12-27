using NetCatHook.Scraper.App.Entities;

namespace NetCatHook.Scraper.App.Repository;

interface ITgBotChatRepository
{
    Task<IEnumerable<TgBotChat>> GetAllAsync();

    Task AddAsync(TgBotChat chat);

    Task<TgBotChat?> GetByChatId(long chatId);
}
