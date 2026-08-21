namespace BusinessLogic.DTOs.Messages.Response;

public class VideoActorRs
{
    public Guid VideoId { get; set; }

    public Guid ActorId { get; set; }

    public bool RoleMain { get; set; }

    public string? Status { get; set; }
}
