namespace CustomerPortal.UserAuthService.Domain.Exceptions;

public class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string message, Exception inner)
        : base(message, inner) { }

    public EntityNotFoundException(string message)
        : base(message) { }

    public EntityNotFoundException() { }
}
