namespace BusinessLogic.Models.View.Request.Query;

public class AccountQr
{
    public bool? IsVerified { get; set; }
    public short Gender { get; set; }
    public string? Nationality { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public string? Role { get; set; }

    public string? Status { get; set; }
}
