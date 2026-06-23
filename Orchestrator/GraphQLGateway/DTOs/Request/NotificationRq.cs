namespace GraphQLGateway.DTOs.Request;

public class NotificationRq
{
    public Guid Id { get; set; }

    public string? Type { get; set; }

    public Guid? TypeId { get; set; }

    public string Title { get; set; } = null!;

    public string? Message { get; set; }

    public bool IsGlobal { get; set; }

    public DateOnly CreatedDate { get; set; }
}