namespace GraphQLGateway.DTOs.Response;

public class AccountBadgeRs
{
    public Guid AccountId { get; set; }

    public Guid BadgeId { get; set; }

    public DateOnly AwardedDate { get; set; }

    public virtual AccountRs? Account { get; set; }

    public virtual BadgeRs? Badge { get; set; }
}
