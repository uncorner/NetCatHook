using NetCatHook.Scraper.App.Entities;

namespace NetCatHook.Scraper.App.Repository;

interface ITgBotChatRepository
{
    Task<IEnumerable<TgBotChat>> GetAllAsync();

    Task AddAsync(TgBotChat chat);

    Task<TgBotChat?> GetByChatId(long chatId);

    //Task AddRangeAsync(IEnumerable<TgBotChat> chat);

    //Task<IEnumerable<TgBotChat>> GetByIdsAsync(IEnumerable<int> ids);

    //Task<IEnumerable<int>> CheckProductsExistAsync(IEnumerable<int> ids);

    //Task BatchRemoveAsync(IEnumerable<int> ids);
}
