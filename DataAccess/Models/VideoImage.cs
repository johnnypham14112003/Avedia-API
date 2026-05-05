using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class VideoImage
{
    public Guid VideoId { get; set; }

    public Guid ImageId { get; set; }

    public bool IsMain { get; set; }

    public short OrderNumerical { get; set; }

    public string? Status { get; set; }

    public virtual Image Image { get; set; } = null!;

    public virtual Video Video { get; set; } = null!;
}
