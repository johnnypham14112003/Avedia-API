namespace BusinessLogic.DTOs.Messages.Request;

public class FavoriteRq
{
    public Guid? AccountId { get; set; }

    public string? TargetType { get; set; }

    public Guid? TargetId { get; set; }
}
