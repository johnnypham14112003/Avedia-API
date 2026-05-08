namespace BusinessLogic.Models.View.Request;

public class AccountRq
{
    public Guid Id { get; set; }

    public string? AvatarUrl { get; set; }

    public string UserName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public bool IsVerified { get; set; }

    public bool? Gender { get; set; }

    public string? Nationality { get; set; }

    public DateOnly JoinedDate { get; set; }

    public int MeritPoint { get; set; }

    public string Role { get; set; } = null!;

    public string Status { get; set; } = null!;
}
