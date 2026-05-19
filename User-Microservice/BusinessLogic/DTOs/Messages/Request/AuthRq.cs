namespace BusinessLogic.DTOs.Messages.Request;

public class AuthRq
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}
