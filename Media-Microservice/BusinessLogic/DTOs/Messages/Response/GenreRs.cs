using DataAccess.Models;

namespace BusinessLogic.DTOs.Messages.Response;

public class GenreRs
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? Status { get; set; }
}
