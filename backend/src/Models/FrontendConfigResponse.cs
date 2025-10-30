namespace OrgChart.API.Models;

/// <summary>
/// Frontend configuration response
/// </summary>
public class FrontendConfigResponse
{
    public MsalConfig Msal { get; set; } = new();
    public ApiConfig Api { get; set; } = new();
    public FeatureConfig Features { get; set; } = new();
}