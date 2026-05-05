using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class Contribution
{
    public Guid Id { get; set; }

    public Guid ContributorId { get; set; }

    public Guid? ApproverId { get; set; }

    public bool AdminReviewed { get; set; }

    public string TargetType { get; set; } = null!;

    public Guid? TargetId { get; set; }

    public string ActionType { get; set; } = null!;

    public DateOnly RequestedDate { get; set; }

    public string? ProposedData { get; set; }

    public string Status { get; set; } = null!;

    public virtual Account? Approver { get; set; }

    public virtual Account Contributor { get; set; } = null!;
}
