using Microsoft.EntityFrameworkCore;
using NetCatHook.Scraper.Application.Repository;
using NetCatHook.Scraper.Domain.Entities;

namespace NetCatHook.Scraper.Infrastructure.Repository;

class DbSubjectChatRepository : ISubjectChatRepository
{
    private readonly ApplicationDbContext dbContext;

    public DbSubjectChatRepository(ApplicationDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task Add(SubjectChat chat)
    {
        await dbContext.SubjectChats.AddAsync(chat);
    }

    public async Task<IEnumerable<SubjectChat>> GetAllEnabled() =>
        await dbContext.SubjectChats.Where(i => i.IsEnabled)
        .OrderBy(i => i.Id).ToArrayAsync();

    public async Task<SubjectChat?> GetByChatId(long chatId)
    {
        return await dbContext.SubjectChats.FirstOrDefaultAsync(i => i.ChatId == chatId);
    }

}
