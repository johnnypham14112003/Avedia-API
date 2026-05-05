using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class VideoActor
{
    public Guid VideoId { get; set; }

    public Guid ActorId { get; set; }

    public bool RoleMain { get; set; }

    public string? Status { get; set; }

    public virtual Actor Actor { get; set; } = null!;

    public virtual Video Video { get; set; } = null!;
}
