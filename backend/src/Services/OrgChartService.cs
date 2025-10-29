using OrgChart.API.Exceptions;
using OrgChart.API.Models;
using OrgChart.API.Repositories;

namespace OrgChart.API.Services;

/// <summary>
/// Service for retrieving organizational chart data from configured repository
/// </summary>
public class OrgChartService : IOrgChartService
{
    private readonly IOrgChartRepository _repository;
    private readonly ILogger<OrgChartService> _logger;

    public OrgChartService(
        IOrgChartRepository repository,
        ILogger<OrgChartService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<OrgChartResponse> GetOrganizationStructureAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving organizational structure from repository");

            var orgChart = await _repository.GetDataAsync(cancellationToken);

            _logger.LogInformation(
                "Successfully loaded organizational structure: {PositionCount} positions, {EmployeeCount} employees (Repository permissions: Insert={InsertEnabled}, Update={UpdateEnabled}, Delete={DeleteEnabled})",
                orgChart.Organization.Positions.Count,
                orgChart.Organization.Positions.Sum(p => p.Employees.Count),
                _repository.InsertEnabled,
                _repository.UpdateEnabled,
                _repository.DeleteEnabled);

            return orgChart;
        }
        catch (DataSourceException ex)
        {
            _logger.LogError(ex, "Repository error while retrieving organizational structure");
            throw new InvalidOperationException($"Failed to retrieve organizational structure: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while retrieving organizational structure");
            throw;
        }
    }
}