namespace BusinessLogic.Models.View.Response;

public class AuthRs
{
    public AccountRs? Account { get; set; }
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
    public required DateTime RefreshExpireTime { get; set; }
}
