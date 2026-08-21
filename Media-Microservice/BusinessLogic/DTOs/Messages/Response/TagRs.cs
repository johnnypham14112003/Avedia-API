namespace BusinessLogic.DTOs.Messages.Response;

public class TagRs
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string Status { get; set; } = null!;
}
