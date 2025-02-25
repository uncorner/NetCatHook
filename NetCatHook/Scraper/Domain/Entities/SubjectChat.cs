using Microsoft.EntityFrameworkCore;

namespace NetCatHook.Scraper.Domain.Entities;

[Index(nameof(ChatId), Name = "IX_ChatId_Unique", IsUnique = true)]
class SubjectChat
{
    public int Id { get; set; }

    public long ChatId { get; set; }

    public bool IsEnabled { get; set; }

    public bool IsNotifying { get; set; }

    public void SetDefault()
    {
        IsEnabled = false;
        IsNotifying = false;
    }

}

