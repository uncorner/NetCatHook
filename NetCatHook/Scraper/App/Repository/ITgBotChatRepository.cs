using NetCatHook.Scraper.App.Entities;

namespace NetCatHook.Scraper.App.Repository;

interface ITgBotChatRepository
{
    Task<IEnumerable<TgBotChat>> GetAllEnabled();

    Task Add(TgBotChat chat);

    Task<TgBotChat?> GetByChatId(long chatId);
}
