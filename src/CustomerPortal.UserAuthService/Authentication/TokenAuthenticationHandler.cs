using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using CustomerPortal.UserAuthService.Domain.Aggregates;
using CustomerPortal.UserAuthService.Domain.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace CustomerPortal.UserAuthService.Authentication;

public class TokenAuthenticationHandler(
    IOptionsMonitor<TokenAuthSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IUserRepository userRepository
) : AuthenticationHandler<TokenAuthSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (Request.Headers.TryGetValue(HeaderNames.Authorization, out var value) is false)
            return AuthenticateResult.NoResult();

        var authHeader = value.ToString();

        if (authHeader.StartsWith("Bearer ") is false)
            return AuthenticateResult.NoResult();

        var userTokenBase64 = authHeader["Bearer ".Length..];
        var userToken = Encoding.UTF8.GetString(Convert.FromBase64String(userTokenBase64));
        var split = userToken.Split(':');

        if (split.Length != 2)
            return AuthenticateResult.NoResult();

        if (Guid.TryParse(split[0], out var userId) is false)
            return AuthenticateResult.NoResult();

        var token = split[1];

        var user = await userRepository.GetById(userId);

        if (user is null)
            return AuthenticateResult.Fail("Invalid token");

        if (user.IsSessionValidAt(token, DateTimeOffset.UtcNow) is false)
            return AuthenticateResult.Fail("Invalid token");

        return AuthenticateResult.Success(
            new AuthenticationTicket(CreateFromUser(user), Scheme.Name)
        );
    }

    private static ClaimsPrincipal CreateFromUser(User user)
    {
        var claimsIdentity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
            ]
        );

        return new ClaimsPrincipal(claimsIdentity);
    }
}
