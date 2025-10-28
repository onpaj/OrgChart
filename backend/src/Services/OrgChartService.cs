using OrgChart.API.DataSources;
using OrgChart.API.Exceptions;
using OrgChart.API.Models;

namespace OrgChart.API.Services;

/// <summary>
/// Service for retrieving organizational chart data from configured data source
/// </summary>
public class OrgChartService : IOrgChartService
{
    private readonly IOrgChartDataSource _dataSource;
    private readonly ILogger<OrgChartService> _logger;

    public OrgChartService(
        IOrgChartDataSource dataSource,
        ILogger<OrgChartService> logger)
    {
        _dataSource = dataSource;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<OrgChartResponse> GetOrganizationStructureAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving organizational structure from data source");

            var orgChart = await _dataSource.GetDataAsync(cancellationToken);

            _logger.LogInformation(
                "Successfully loaded organizational structure: {PositionCount} positions, {EmployeeCount} employees",
                orgChart.Organization.Positions.Count,
                orgChart.Organization.Positions.Sum(p => p.Employees.Count));

            return orgChart;
        }
        catch (DataSourceException ex)
        {
            _logger.LogError(ex, "Data source error while retrieving organizational structure");
            throw new InvalidOperationException($"Failed to retrieve organizational structure: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while retrieving organizational structure");
            throw;
        }
    }
}