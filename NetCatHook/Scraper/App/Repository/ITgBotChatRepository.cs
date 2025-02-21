using NetCatHook.Scraper.Domain.Entities;

namespace NetCatHook.Scraper.App.Repository;

interface ITgBotChatRepository
{
    Task<IEnumerable<TgBotChat>> GetAllEnabled();

    Task Add(TgBotChat chat);

    Task<TgBotChat?> GetByChatId(long chatId);
}
