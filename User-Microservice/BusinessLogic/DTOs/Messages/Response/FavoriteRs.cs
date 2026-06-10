namespace BusinessLogic.DTOs.Messages.Response;

public class FavoriteRs
{
    public Guid Id { get; set; }

    public Guid LoverId { get; set; }

    public string TargetType { get; set; } = null!;

    public Guid TargetId { get; set; }
}