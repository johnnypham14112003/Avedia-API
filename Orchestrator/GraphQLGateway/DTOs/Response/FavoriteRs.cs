namespace GraphQLGateway.DTOs.Response;

public class FavoriteRs
{
    public Guid Id { get; set; }

    public Guid AccountId { get; set; }

    public string TargetType { get; set; } = null!;

    public Guid TargetId { get; set; }
}