namespace BusinessLogic.Extensions.Exceptions;

public class NotFoundException(string message) : Exception(message)
{
}