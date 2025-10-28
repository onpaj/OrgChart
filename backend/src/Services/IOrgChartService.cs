using OrgChart.API.Models;

namespace OrgChart.API.Services;

/// <summary>
/// Service for retrieving organizational chart data
/// </summary>
public interface IOrgChartService
{
    /// <summary>
    /// Retrieves the complete organizational structure from external data source
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Organization chart data</returns>
    Task<OrgChartResponse> GetOrganizationStructureAsync(CancellationToken cancellationToken = default);
}