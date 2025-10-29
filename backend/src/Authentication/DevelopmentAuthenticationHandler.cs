using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace OrgChart.API.Authentication;

/// <summary>
/// Development-only authentication handler that creates fake user claims
/// </summary>
public class DevelopmentAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public DevelopmentAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Context.Request.Headers.ContainsKey("X-Development-User"))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var userHeader = Context.Request.Headers["X-Development-User"].FirstOrDefault();
        if (string.IsNullOrEmpty(userHeader))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        // Create fake claims based on header value
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userHeader),
            new Claim(ClaimTypes.Name, $"Dev User {userHeader}"),
            new Claim("IdentityProvider", "Development")
        };

        // In development, give everyone OrgChart_Write claim for easier testing
        claims.Add(new Claim("OrgChart_Write", "true"));
        
        // Or keep the selective approach:
        // if (userHeader.Contains("admin") || userHeader.Contains("editor"))
        // {
        //     claims.Add(new Claim("OrgChart_Write", "true"));
        // }

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        Logger.LogInformation("Development authentication for user: {User} with claims: {Claims}", 
            userHeader, string.Join(", ", claims.Select(c => $"{c.Type}={c.Value}")));

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}