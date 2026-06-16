namespace BusinessLogic.DTOs.Messages.Response;

public class ContributionRs
{
    public Guid Id { get; set; }

    public Guid ContributorId { get; set; }

    public Guid? ApproverId { get; set; }

    public bool AdminApproved { get; set; }

    public string TargetType { get; set; } = null!;

    public Guid? TargetId { get; set; }

    public string ActionType { get; set; } = null!;

    public DateOnly? HandledDate { get; set; }

    public DateOnly RequestedDate { get; set; }

    public string? ProposedData { get; set; }

    public string Status { get; set; } = null!;
}