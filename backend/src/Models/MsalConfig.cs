namespace OrgChart.API.Models;

/// <summary>
/// MSAL authentication configuration
/// </summary>
public class MsalConfig
{
    public string ClientId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string Authority { get; set; } = string.Empty;
    public string BackendClientId { get; set; } = string.Empty;
}