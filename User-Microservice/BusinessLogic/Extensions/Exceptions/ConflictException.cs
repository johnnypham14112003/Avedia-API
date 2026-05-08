namespace BusinessLogic.Extensions.Exceptions;

public class ConflictException(string message) : Exception(message)
{
    //409: Cannot complete because existed
}
