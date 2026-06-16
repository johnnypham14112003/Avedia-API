namespace BusinessLogic.DTOs.Messages.Request;

public class ContributionRq
{
    public Guid Id { get; set; }

    public Guid ContributorId { get; set; }

    public Guid? ApproverId { get; set; }

    public bool AdminApproved { get; set; }

    public string? TargetType { get; set; }

    public Guid? TargetId { get; set; }

    public string ActionType { get; set; } = null!;

    public DateOnly? HandledDate { get; set; }

    public string? ProposedData { get; set; }

    public string? Status { get; set; }
}