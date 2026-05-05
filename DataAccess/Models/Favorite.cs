using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class Favorite
{
    public Guid Id { get; set; }

    public Guid LoverId { get; set; }

    public string TargetType { get; set; } = null!;

    public Guid? TargetId { get; set; }

    public virtual Account Lover { get; set; } = null!;
}
