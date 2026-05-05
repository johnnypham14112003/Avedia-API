using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class Label
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<Video> Videos { get; set; } = new List<Video>();
}
