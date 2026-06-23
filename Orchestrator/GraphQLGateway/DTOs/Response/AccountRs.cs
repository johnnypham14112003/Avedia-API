namespace GraphQLGateway.DTOs.Response;

public class AccountRs
{
    public Guid Id { get; set; }

    public string? AvatarUrl { get; set; }

    public string? OtpCode { get; set; }

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
    public virtual ICollection<AccountBadgeRs>? AccountBadges { get; set; }
}
