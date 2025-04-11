using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace CustomerPortal.UserAuthService.Authentication;

public class TokenAuthenticationHandler : AuthenticationHandler<TokenAuthSchemeOptions>
{
    public TokenAuthenticationHandler(
        IOptionsMonitor<TokenAuthSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder
    )
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey(HeaderNames.Authorization))
            return Task.FromResult(AuthenticateResult.NoResult());

        var authHeader = Request.Headers[HeaderNames.Authorization].ToString();

        if (authHeader.StartsWith("Bearer ") is false)
            return Task.FromResult(AuthenticateResult.NoResult());

        var token = authHeader["Bearer ".Length..];

        // Token should contain the userId and the token, probably. Need to think of a concept for this.

        throw new NotImplementedException();
    }
}
