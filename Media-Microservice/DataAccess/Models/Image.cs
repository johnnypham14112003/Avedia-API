using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class Image
{
    public Guid Id { get; set; }

    public string? LocalePath { get; set; }

    public string Url { get; set; } = null!;

    public DateOnly UploadDate { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<ActorImage> ActorImages { get; set; } = new List<ActorImage>();

    public virtual ICollection<VideoImage> VideoImages { get; set; } = new List<VideoImage>();
}
