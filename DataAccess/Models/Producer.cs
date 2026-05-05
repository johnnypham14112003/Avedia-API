using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class Producer
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? OtherName { get; set; }

    public string? Description { get; set; }

    public DateOnly? EstablishDate { get; set; }

    public string? Country { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<Video> Videos { get; set; } = new List<Video>();
}
