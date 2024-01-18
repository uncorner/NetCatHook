using NetCatHook.Scraper.App.Entities;

namespace NetCatHook.Scraper.App.Repository;

interface ITgBotChatRepository
{
    Task<IEnumerable<TgBotChat>> GetAll();

    Task Add(TgBotChat chat);

    Task<TgBotChat?> GetByChatId(long chatId);
}
