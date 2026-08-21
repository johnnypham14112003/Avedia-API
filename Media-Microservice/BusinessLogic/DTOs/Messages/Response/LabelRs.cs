namespace BusinessLogic.DTOs.Messages.Response;

public class LabelRs
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string Status { get; set; } = null!;
}
