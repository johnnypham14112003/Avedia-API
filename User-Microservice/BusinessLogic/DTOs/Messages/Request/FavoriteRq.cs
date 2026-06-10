namespace BusinessLogic.DTOs.Messages.Request;

public class FavoriteRq
{
    public Guid? LoverId { get; set; }

    public string? TargetType { get; set; }

    public Guid? TargetId { get; set; }
}
