namespace GraphQLGateway.DTOs.Request.Query;

public class NotificationQr
{
    public Guid? AccountId { get; set; }
    public string? Type { get; set; }
    public Guid? TypeId { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public bool? IsGlobal { get; set; }
}