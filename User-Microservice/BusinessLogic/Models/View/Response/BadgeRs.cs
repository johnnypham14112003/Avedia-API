using DataAccess.Models;

namespace BusinessLogic.Models.View.Response;

public class BadgeRs
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? IconUrl { get; set; }

    public string? LocalePath { get; set; }

    public short RareLevel { get; set; }

    public DateOnly CreatedDate { get; set; }

    public string Status { get; set; } = null!;
    public virtual ICollection<AccountBadgeRs>? AccountBadges { get; set; }
}
