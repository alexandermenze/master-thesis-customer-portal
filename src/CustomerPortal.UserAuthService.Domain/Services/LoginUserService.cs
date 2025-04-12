using CustomerPortal.UserAuthService.Domain.Aggregates;
using CustomerPortal.UserAuthService.Domain.Repositories;

namespace CustomerPortal.UserAuthService.Domain.Services;

public class LoginUserService(
    IUserRepository userRepository,
    IPasswordService passwordService,
    ITokenGenerationService tokenGenerationService,
    TimeProvider timeProvider
) : ILoginUserService
{
    public async Task<SessionToken?> Login(string email, string password)
    {
        var user = await userRepository.GetByEmail(email);

        if (user is null)
            return null;

        var passwordWithHashAndSalt = passwordService.HashPassword(user.Email, password);

        if (user.Authenticate(user.Email, passwordWithHashAndSalt) is false)
            return null;

        var token = tokenGenerationService.Generate();

        var sessionToken = user.AddSessionToken(
            token,
            timeProvider.GetUtcNow().AddHours(1).DateTime
        );

        await userRepository.Save(user);

        return sessionToken;
    }
}
