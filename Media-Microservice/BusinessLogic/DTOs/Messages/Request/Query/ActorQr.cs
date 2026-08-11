namespace BusinessLogic.DTOs.Messages.Request.Query;

public class ActorQr
{
    public bool? Gender { get; set; }

    public string? Height { get; set; }

    public string? CupSize { get; set; }

    public string? YearOfBirth { get; set; }

    public string? Size { get; set; }

    public DateOnly? FromDebutDate { get; set; }
    public DateOnly? ToDebutDate { get; set; }

    public string? Nationality { get; set; }

    public string? Company { get; set; }

    public string? Status { get; set; }
    public short? OrderBy { get; set; } = 0;
}
