using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class Badge
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? IconUrl { get; set; }

    public string? LocalePath { get; set; }

    public short RareLevel { get; set; }

    public DateOnly CreatedDate { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<AccountBadge> AccountBadges { get; set; } = new List<AccountBadge>();
}
