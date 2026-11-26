using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using OrgChart.API.Configuration;

namespace OrgChart.API.Services;

/// <summary>Exchanges the caller's OrgChart API token for a Heblo token (Entra on-behalf-of).</summary>
public interface IHebloTokenProvider
{
    Task<string> AcquireTokenOnBehalfOfAsync(string userAccessToken, CancellationToken cancellationToken);
}

public sealed class HebloTokenProvider : IHebloTokenProvider
{
    private readonly IConfidentialClientApplication _app;
    private readonly string[] _scopes;

    public HebloTokenProvider(IConfiguration configuration, IOptions<HebloOptions> options)
    {
        var tenantId = configuration["AzureAd:TenantId"];
        var clientId = configuration["AzureAd:ClientId"];
        var clientSecret = configuration["AzureAd:ClientSecret"];
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
        {
            throw new InvalidOperationException(
                "Azure AD configuration is missing. Configure AzureAd:TenantId, ClientId and ClientSecret.");
        }

        // MSAL keeps an in-memory token cache per application instance, so this is a singleton.
        _app = ConfidentialClientApplicationBuilder.Create(clientId)
            .WithClientSecret(clientSecret)
            .WithAuthority(AzureCloudInstance.AzurePublic, tenantId)
            .Build();
        _scopes = new[] { options.Value.Scope };
    }

    public async Task<string> AcquireTokenOnBehalfOfAsync(string userAccessToken, CancellationToken cancellationToken)
    {
        var result = await _app
            .AcquireTokenOnBehalfOf(_scopes, new UserAssertion(userAccessToken))
            .ExecuteAsync(cancellationToken);
        return result.AccessToken;
    }
}
