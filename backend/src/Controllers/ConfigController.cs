using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrgChart.API.Models;

namespace OrgChart.API.Controllers;

/// <summary>
/// Controller for providing frontend configuration
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // This endpoint must be accessible without authentication
public class ConfigController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConfigController> _logger;

    public ConfigController(IConfiguration configuration, ILogger<ConfigController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Gets the frontend configuration including MSAL settings
    /// </summary>
    /// <returns>Frontend configuration</returns>
    /// <response code="200">Returns the frontend configuration</response>
    [HttpGet]
    [ProducesResponseType(typeof(FrontendConfigResponse), StatusCodes.Status200OK)]
    public ActionResult<FrontendConfigResponse> GetConfig()
    {
        try
        {
            var config = new FrontendConfigResponse
            {
                Msal = new MsalConfig
                {
                    ClientId = _configuration["Frontend:ClientId"] ?? string.Empty,
                    TenantId = _configuration["AzureAd:TenantId"] ?? string.Empty,
                    Authority = $"https://login.microsoftonline.com/{_configuration["AzureAd:TenantId"] ?? string.Empty}",
                    BackendClientId = _configuration["AzureAd:ClientId"] ?? string.Empty
                },
                Api = new ApiConfig
                {
                    BaseUrl = "/api"
                },
                Features = new FeatureConfig
                {
                    AuthenticationEnabled = !_configuration.GetValue<bool>("UseMockAuth", false)
                }
            };

            _logger.LogInformation("Returning frontend configuration");
            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting frontend configuration");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "Failed to get configuration" });
        }
    }
}