namespace GraphQLGateway.DTOs.Request;

public class AuthRq
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}
