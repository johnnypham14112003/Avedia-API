using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class AccountBadge
{
    public Guid AccountId { get; set; }

    public Guid BadgeId { get; set; }

    public DateOnly AwardedDate { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Badge Badge { get; set; } = null!;
}
