using CustomerPortal.UserAuthService.Domain.DataClasses;
using CustomerPortal.UserAuthService.Domain.Exceptions;

namespace CustomerPortal.UserAuthService.Domain.Aggregates;

public class User(Guid id, UserData userData)
{
    // Required private constructor for EntityFramework
    private User()
        : this(Guid.Empty, new UserData(null!, null!, null!, null!)) { }

    public Guid Id { get; private set; } = id;
    public string Email { get; private set; } = userData.Email;
    public string PasswordHashWithSalt { get; private set; } = userData.PasswordHashWithSalt;
    public string FirstName { get; private set; } = userData.FirstName;
    public string LastName { get; private set; } = userData.LastName;
    public bool Approved { get; private set; }

    private readonly List<SessionToken> _sessionTokens = [];
    public IReadOnlyList<SessionToken> SessionTokens => _sessionTokens.AsReadOnly();

    public void Approve()
    {
        if (Approved)
            throw new DomainValidationException("User is already approved.");

        Approved = true;
    }

    public bool Authenticate(string email, string passwordHashWithSalt)
    {
        return Email.Equals(email, StringComparison.OrdinalIgnoreCase)
            && PasswordHashWithSalt.Equals(passwordHashWithSalt, StringComparison.Ordinal);
    }

    public SessionToken AddSessionToken(string token, DateTime expiresAt)
    {
        var sessionToken = new SessionToken(Guid.CreateVersion7(), token, expiresAt);
        _sessionTokens.Add(sessionToken);
        return sessionToken;
    }

    public bool IsSessionValidAt(string token, DateTime when)
    {
        return _sessionTokens
            .Where(s => s.Token.Equals(token, StringComparison.Ordinal))
            .Any(s => s.ExpiresAt < when);
    }
}
