namespace BusinessLogic.DTOs.Messages.Request;

public class GenreRq
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? Status { get; set; }
}
