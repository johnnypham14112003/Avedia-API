namespace BusinessLogic.DTOs.Messages.Request.Query;

public class BadgeQr
{
    public short? RareLevel { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public string? Status { get; set; }
}
