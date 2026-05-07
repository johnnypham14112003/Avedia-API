namespace BusinessLogic.Models.View.Request;

public class RefreshTokenRq
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
}
