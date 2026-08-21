namespace BusinessLogic.DTOs.Messages.Response;

public class VideoImageRs
{
    public Guid VideoId { get; set; }

    public Guid ImageId { get; set; }

    public bool IsMain { get; set; }

    public short OrderNumerical { get; set; }

    public string? Status { get; set; }
}
