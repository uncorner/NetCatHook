using NetCatHook.Scraper.Domain.Entities;

namespace NetCatHook.Scraper.Application.Repository;

interface ISubjectChatRepository
{
    Task<IEnumerable<SubjectChat>> GetAllEnabled();

    Task Add(SubjectChat chat);

    Task<SubjectChat?> GetByChatId(long chatId);
}
