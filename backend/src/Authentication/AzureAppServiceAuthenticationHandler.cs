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
        try
        {
            // Log all headers for debugging
            Logger.LogInformation("Available headers: {Headers}", 
                string.Join(", ", Request.Headers.Select(h => $"{h.Key}={h.Value}")));

            // Check for Azure App Service Easy Auth headers
            var clientPrincipalHeader = Request.Headers["X-MS-CLIENT-PRINCIPAL"].FirstOrDefault();
            
            if (!string.IsNullOrEmpty(clientPrincipalHeader))
            {
                Logger.LogInformation("Found X-MS-CLIENT-PRINCIPAL header");
                return ProcessClientPrincipal(clientPrincipalHeader);
            }

            // Alternative: Check for authentication cookies
            var authCookie = Request.Cookies["AppServiceAuthSession"];
            if (!string.IsNullOrEmpty(authCookie))
            {
                Logger.LogInformation("Found AppServiceAuthSession cookie");
                // For cookie-based auth, we might need to call /.auth/me internally
                return ProcessAuthCookie();
            }

            // Check if user is authenticated via other Azure mechanisms
            var userPrincipal = Request.Headers["X-MS-CLIENT-PRINCIPAL-NAME"].FirstOrDefault();
            var userIdHeader = Request.Headers["X-MS-CLIENT-PRINCIPAL-ID"].FirstOrDefault();
            
            if (!string.IsNullOrEmpty(userPrincipal) || !string.IsNullOrEmpty(userIdHeader))
            {
                Logger.LogInformation("Found alternative Azure headers");
                return ProcessAlternativeHeaders(userPrincipal, userIdHeader);
            }

            Logger.LogInformation("No Azure App Service authentication headers found");
            return Task.FromResult(AuthenticateResult.NoResult());
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in AzureAppServiceAuthenticationHandler");
            return Task.FromResult(AuthenticateResult.Fail($"Authentication error: {ex.Message}"));
        }
    }

    private Task<AuthenticateResult> ProcessClientPrincipal(string clientPrincipalHeader)
    {
        try
        {
            // Decode the base64 encoded client principal
            var decodedBytes = Convert.FromBase64String(clientPrincipalHeader);
            var decodedJson = System.Text.Encoding.UTF8.GetString(decodedBytes);
            
            Logger.LogInformation("Decoded client principal: {Json}", decodedJson);
            
            var clientPrincipal = JsonSerializer.Deserialize<ClientPrincipal>(decodedJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (clientPrincipal?.UserId == null)
            {
                Logger.LogWarning("Client principal does not contain valid UserId");
                return Task.FromResult(AuthenticateResult.Fail("Invalid client principal"));
            }

            var claims = CreateClaimsFromPrincipal(clientPrincipal);
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            Logger.LogInformation("Successfully authenticated user: {UserId}", clientPrincipal.UserId);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing client principal");
            return Task.FromResult(AuthenticateResult.Fail("Error processing client principal"));
        }
    }

    private Task<AuthenticateResult> ProcessAuthCookie()
    {
        // For cookie-based authentication, we might need to make an internal call to /.auth/me
        // This is a simplified approach - you might need to implement actual cookie validation
        Logger.LogInformation("Cookie-based authentication detected but not fully implemented");
        return Task.FromResult(AuthenticateResult.NoResult());
    }

    private Task<AuthenticateResult> ProcessAlternativeHeaders(string? userPrincipal, string? userId)
    {
        // Create basic claims from alternative headers
        var claims = new List<Claim>();
        
        if (!string.IsNullOrEmpty(userId))
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
        }
        
        if (!string.IsNullOrEmpty(userPrincipal))
        {
            claims.Add(new Claim(ClaimTypes.Name, userPrincipal));
        }

        if (claims.Count == 0)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        claims.Add(new Claim("IdentityProvider", "AzureAppService"));
        
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        Logger.LogInformation("Authenticated via alternative headers: {User}", userPrincipal ?? userId);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private List<Claim> CreateClaimsFromPrincipal(ClientPrincipal clientPrincipal)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, clientPrincipal.UserId!),
            new Claim(ClaimTypes.Name, clientPrincipal.UserDetails ?? clientPrincipal.UserId!),
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

        return claims;
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