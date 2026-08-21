namespace BusinessLogic.DTOs.ElasticDocuments;

public class VideoDocument
{
    public Guid Id { get; set; }

    public string? Code { get; set; }

    public string Title { get; set; } = null!;

    public string OriginalTitle { get; set; } = null!;

    public int DurationMinutes { get; set; }

    public string? Series { get; set; }

    public string? Director { get; set; }

    public DateOnly? ReleaseDate { get; set; }

    public string? Language { get; set; }

    public string Status { get; set; } = null!;
    public int LikeCount { get; set; }

    public List<Guid> GenreIds { get; set; } = [];
    public List<Guid> TagIds { get; set; } = [];
}
