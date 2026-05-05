using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class Account
{
    public Guid Id { get; set; }

    public string? AvatarUrl { get; set; }

    public string? JwtSession { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpirytime { get; set; }

    public string UserName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public bool IsVerified { get; set; }

    public string PasswordHash { get; set; } = null!;

    public bool? Gender { get; set; }

    public string? Nationality { get; set; }

    public DateOnly JoinedDate { get; set; }

    public int MeritPoint { get; set; }

    public string Role { get; set; } = null!;

    public string Status { get; set; } = null!;

    public virtual ICollection<AccountBadge> AccountBadges { get; set; } = new List<AccountBadge>();

    public virtual ICollection<AccountNotification> AccountNotifications { get; set; } = new List<AccountNotification>();

    public virtual ICollection<Contribution> ContributionApprovers { get; set; } = new List<Contribution>();

    public virtual ICollection<Contribution> ContributionContributors { get; set; } = new List<Contribution>();

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
}
