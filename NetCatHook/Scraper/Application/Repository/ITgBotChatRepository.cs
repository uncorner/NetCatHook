using NetCatHook.Scraper.Domain.Entities;

namespace NetCatHook.Scraper.Application.Repository;

interface ITgBotChatRepository
{
    Task<IEnumerable<TgBotChat>> GetAllEnabled();

    Task Add(TgBotChat chat);

    Task<TgBotChat?> GetByChatId(long chatId);
}
