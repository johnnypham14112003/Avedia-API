using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class Video
{
    public Guid Id { get; set; }

    public string? Code { get; set; }

    public string Title { get; set; } = null!;

    public string? OriginalTitle { get; set; }

    public string? Description { get; set; }

    public int DurationMinutes { get; set; }

    public string? Series { get; set; }

    public short Episode { get; set; }

    public string? Director { get; set; }

    public DateOnly? ReleaseDate { get; set; }

    public string? Language { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<VideoActor> VideoActors { get; set; } = new List<VideoActor>();

    public virtual ICollection<VideoImage> VideoImages { get; set; } = new List<VideoImage>();

    public virtual ICollection<Genre> Genres { get; set; } = new List<Genre>();

    public virtual ICollection<Label> Labels { get; set; } = new List<Label>();

    public virtual ICollection<Producer> Producers { get; set; } = new List<Producer>();

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
