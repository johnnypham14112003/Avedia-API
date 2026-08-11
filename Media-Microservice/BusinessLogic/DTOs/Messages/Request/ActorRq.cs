namespace BusinessLogic.DTOs.Messages.Request;

public class ActorRq
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = null!;

    public string? StageName { get; set; }

    public bool? Gender { get; set; }

    public DateOnly? Dob { get; set; }

    public string? Bio { get; set; }

    public string? Height { get; set; }

    public string? CupSize { get; set; }

    public string? Size { get; set; }

    public DateOnly? DebutDate { get; set; }

    public string? Nationality { get; set; }

    public string? Company { get; set; }

    public string? Status { get; set; }

    public string? AvatarUrl { get; set; }
}
