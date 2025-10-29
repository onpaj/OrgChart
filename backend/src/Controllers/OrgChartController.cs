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

            // Check if user has OrgChart_Write claim
            var canEdit = false;
            if (authEnabled && User.Identity?.IsAuthenticated == true)
            {
                canEdit = User.HasClaim(c => c.Type == "OrgChart_Write");
            }
            else if (!authEnabled)
            {
                // If authentication is disabled, allow editing
                canEdit = true;
            }

            result.Permissions = new UserPermissions { CanEdit = canEdit };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching organizational structure");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to fetch organizational structure", message = ex.Message });
        }
    }

    /// <summary>
    /// Creates a new position in the organizational structure
    /// </summary>
    /// <param name="request">Position creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created position</returns>
    /// <response code="201">Position created successfully</response>
    /// <response code="401">Unauthorized - missing or invalid authentication</response>
    /// <response code="403">Forbidden - insufficient permissions</response>
    /// <response code="400">Bad request - invalid data</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("positions")]
    [Authorize(Policy = "OrgChartWritePolicy")]
    [ProducesResponseType(typeof(Position), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Position>> CreatePosition([FromBody] CreatePositionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating position: {Title}", request.Title);
            var position = await _orgChartService.CreatePositionAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetOrganizationStructure), new { id = position.Id }, position);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating position");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to create position", message = ex.Message });
        }
    }

    /// <summary>
    /// Updates an existing position in the organizational structure
    /// </summary>
    /// <param name="id">Position ID</param>
    /// <param name="request">Position update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated position</returns>
    /// <response code="200">Position updated successfully</response>
    /// <response code="401">Unauthorized - missing or invalid authentication</response>
    /// <response code="403">Forbidden - insufficient permissions</response>
    /// <response code="404">Position not found</response>
    /// <response code="400">Bad request - invalid data</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("positions/{id}")]
    [Authorize(Policy = "OrgChartWritePolicy")]
    [ProducesResponseType(typeof(Position), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Position>> UpdatePosition(string id, [FromBody] UpdatePositionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Updating position: {PositionId}", id);
            var position = await _orgChartService.UpdatePositionAsync(id, request, cancellationToken);
            return Ok(position);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating position: {PositionId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to update position", message = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a position from the organizational structure
    /// </summary>
    /// <param name="id">Position ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content if successful</returns>
    /// <response code="204">Position deleted successfully</response>
    /// <response code="401">Unauthorized - missing or invalid authentication</response>
    /// <response code="403">Forbidden - insufficient permissions</response>
    /// <response code="404">Position not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("positions/{id}")]
    [Authorize(Policy = "OrgChartWritePolicy")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeletePosition(string id, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Deleting position: {PositionId}", id);
            await _orgChartService.DeletePositionAsync(id, cancellationToken);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting position: {PositionId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to delete position", message = ex.Message });
        }
    }

    /// <summary>
    /// Creates a new employee
    /// </summary>
    /// <param name="request">Employee creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created employee</returns>
    /// <response code="201">Employee created successfully</response>
    /// <response code="401">Unauthorized - missing or invalid authentication</response>
    /// <response code="403">Forbidden - insufficient permissions</response>
    /// <response code="400">Bad request - invalid data</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("employees")]
    [Authorize(Policy = "OrgChartWritePolicy")]
    [ProducesResponseType(typeof(Employee), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Employee>> CreateEmployee([FromBody] CreateEmployeeRequest request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating employee: {Name}", request.Name);
            var employee = await _orgChartService.CreateEmployeeAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetOrganizationStructure), new { id = employee.Id }, employee);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating employee");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to create employee", message = ex.Message });
        }
    }

    /// <summary>
    /// Updates an existing employee
    /// </summary>
    /// <param name="id">Employee ID</param>
    /// <param name="request">Employee update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated employee</returns>
    /// <response code="200">Employee updated successfully</response>
    /// <response code="401">Unauthorized - missing or invalid authentication</response>
    /// <response code="403">Forbidden - insufficient permissions</response>
    /// <response code="404">Employee not found</response>
    /// <response code="400">Bad request - invalid data</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("employees/{id}")]
    [Authorize(Policy = "OrgChartWritePolicy")]
    [ProducesResponseType(typeof(Employee), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Employee>> UpdateEmployee(string id, [FromBody] UpdateEmployeeRequest request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Updating employee: {EmployeeId}", id);
            var employee = await _orgChartService.UpdateEmployeeAsync(id, request, cancellationToken);
            return Ok(employee);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating employee: {EmployeeId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to update employee", message = ex.Message });
        }
    }

    /// <summary>
    /// Deletes an employee
    /// </summary>
    /// <param name="id">Employee ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content if successful</returns>
    /// <response code="204">Employee deleted successfully</response>
    /// <response code="401">Unauthorized - missing or invalid authentication</response>
    /// <response code="403">Forbidden - insufficient permissions</response>
    /// <response code="404">Employee not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("employees/{id}")]
    [Authorize(Policy = "OrgChartWritePolicy")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteEmployee(string id, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Deleting employee: {EmployeeId}", id);
            await _orgChartService.DeleteEmployeeAsync(id, cancellationToken);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting employee: {EmployeeId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to delete employee", message = ex.Message });
        }
    }
}