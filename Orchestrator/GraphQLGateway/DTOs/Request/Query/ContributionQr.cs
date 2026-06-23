namespace GraphQLGateway.DTOs.Request.Query;

public class ContributionQr
{
    public Guid? ContributorId { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    /// <summary>
    /// <c>false</c>: request date | <c>true</c>: handle date
    /// </summary>
    public bool? ByTypeDate { get; set; }// For order and query
    public Guid? TargetId { get; set; }
    public bool? AdminApproved { get; set; }
    public string? Status { get; set; }
}