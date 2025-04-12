using CustomerPortal.UserAuthService.Domain.Aggregates;

namespace CustomerPortal.UserAuthService.Domain.Services;

public interface ILoginUserService
{
    Task<SessionToken?> Login(string email, string password);
}
