namespace CustomerPortal.UserAuthService.Domain.Aggregates;

public class SessionToken(Guid id, string token, DateTime expiresAt)
{
    private SessionToken()
        : this(Guid.Empty, null!, default) { }

    public Guid Id { get; private set; } = id;
    public string Token { get; private set; } = token;
    public DateTime ExpiresAt { get; private set; } = expiresAt;
}
