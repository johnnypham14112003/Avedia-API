using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class Genre
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<Video> Videos { get; set; } = new List<Video>();
}
