using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace OrgChart.API.Authentication;

public class AzureAppServiceAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public AzureAppServiceAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check if we're running in Azure App Service with Easy Auth
        if (!Request.Headers.ContainsKey("X-MS-CLIENT-PRINCIPAL"))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        try
        {
            var clientPrincipalHeader = Request.Headers["X-MS-CLIENT-PRINCIPAL"].FirstOrDefault();
            if (string.IsNullOrEmpty(clientPrincipalHeader))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            // Decode the base64 encoded client principal
            var decodedBytes = Convert.FromBase64String(clientPrincipalHeader);
            var decodedJson = System.Text.Encoding.UTF8.GetString(decodedBytes);
            
            var clientPrincipal = JsonSerializer.Deserialize<ClientPrincipal>(decodedJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (clientPrincipal?.UserId == null)
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid client principal"));
            }

            // Create claims from the client principal
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, clientPrincipal.UserId),
                new Claim(ClaimTypes.Name, clientPrincipal.UserDetails ?? clientPrincipal.UserId),
                new Claim("IdentityProvider", clientPrincipal.IdentityProvider ?? "unknown")
            };

            // Add user roles as claims
            if (clientPrincipal.UserRoles != null)
            {
                foreach (var role in clientPrincipal.UserRoles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            // Add custom claims
            if (clientPrincipal.Claims != null)
            {
                foreach (var claim in clientPrincipal.Claims)
                {
                    claims.Add(new Claim(claim.Typ, claim.Val));
                }
            }

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing Azure App Service client principal");
            return Task.FromResult(AuthenticateResult.Fail("Error processing client principal"));
        }
    }
}

public class ClientPrincipal
{
    public string? IdentityProvider { get; set; }
    public string? UserId { get; set; }
    public string? UserDetails { get; set; }
    public string[]? UserRoles { get; set; }
    public ClientPrincipalClaim[]? Claims { get; set; }
}

public class ClientPrincipalClaim
{
    public string Typ { get; set; } = string.Empty;
    public string Val { get; set; } = string.Empty;
}