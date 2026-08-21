namespace BusinessLogic.DTOs.Messages.Request.Query;

public class VideoQr
{
    public Guid Id { get; set; }

    public string? Code { get; set; }

    public string Title { get; set; } = null!;

    public string OriginalTitle { get; set; } = null!;

    public int? DurationMinutes { get; set; }

    public string? Series { get; set; }

    public string? Director { get; set; }

    public DateOnly? FromReleaseDate { get; set; }
    public DateOnly? ToReleaseDate { get; set; }

    public string? Language { get; set; }

    public string Status { get; set; } = null!;

    public short? OrderBy { get; set; } = 0;
    public Guid? GenreId { get; set; }
    public Guid? TagId { get; set; }
}
