using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrgChart.API.Models;
using OrgChart.API.Services;

namespace OrgChart.API.Controllers;

/// <summary>
/// Controller for organizational chart operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OrgChartController : ControllerBase
{
    private readonly IOrgChartService _orgChartService;
    private readonly ILogger<OrgChartController> _logger;
    private readonly IConfiguration _configuration;

    public OrgChartController(
        IOrgChartService orgChartService,
        ILogger<OrgChartController> logger,
        IConfiguration configuration)
    {
        _orgChartService = orgChartService;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Gets the complete organizational structure
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Organization chart with all positions and employees</returns>
    /// <response code="200">Returns the organizational structure</response>
    /// <response code="500">If there was an error fetching the data</response>
    [HttpGet]
    [ProducesResponseType(typeof(OrgChartResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrgChartResponse>> GetOrganizationStructure(CancellationToken cancellationToken)
    {
        // Apply authorization only if authentication is enabled
        var authEnabled = _configuration.GetValue<bool>("Authentication:Enabled");
        if (authEnabled && !(User.Identity?.IsAuthenticated == true))
        {
            return Unauthorized();
        }

        try
        {
            _logger.LogInformation("Fetching organizational structure");
            var result = await _orgChartService.GetOrganizationStructureAsync(cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching organizational structure");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to fetch organizational structure", message = ex.Message });
        }
    }
}