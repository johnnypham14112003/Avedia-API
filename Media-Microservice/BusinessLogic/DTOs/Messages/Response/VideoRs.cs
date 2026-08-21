namespace BusinessLogic.DTOs.Messages.Response;

public class VideoRs
{
    public Guid Id { get; set; }

    public string? Code { get; set; }

    public string Title { get; set; } = null!;

    public string OriginalTitle { get; set; } = null!;

    public string? Description { get; set; }

    public int DurationMinutes { get; set; }

    public string? Series { get; set; }

    public short Episode { get; set; }

    public string? Director { get; set; }

    public DateOnly? ReleaseDate { get; set; }

    public string? Language { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<VideoActorRs> VideoActors { get; set; } = [];
    public virtual ICollection<VideoImageRs> VideoImages { get; set; } = [];
    public virtual ICollection<GenreRs> Genres { get; set; } = [];
    public virtual ICollection<LabelRs> Labels { get; set; } = [];
    public virtual ICollection<ProducerRs> Producers { get; set; } = [];
    public virtual ICollection<TagRs> Tags { get; set; } = [];
}
