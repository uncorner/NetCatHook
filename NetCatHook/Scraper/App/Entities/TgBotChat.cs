using Microsoft.EntityFrameworkCore;

namespace NetCatHook.Scraper.App.Entities;

[Index(nameof(ChatId), Name = "IX_ChatId_Unique", IsUnique = true)]
class TgBotChat
{
    public int Id { get; set; }

    public long ChatId { get; set; }

}

