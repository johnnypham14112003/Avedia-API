using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class ActorImage
{
    public Guid ActorId { get; set; }

    public Guid ImageId { get; set; }

    public bool IsMain { get; set; }

    public short OrderNumerical { get; set; }

    public string? Status { get; set; }

    public virtual Actor Actor { get; set; } = null!;

    public virtual Image Image { get; set; } = null!;
}
