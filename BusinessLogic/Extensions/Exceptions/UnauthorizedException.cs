namespace BusinessLogic.Extensions.Exceptions;

public class UnauthorizedException(string message) : Exception(message)
{
}
