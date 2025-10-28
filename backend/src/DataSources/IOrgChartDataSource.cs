using OrgChart.API.Models;

namespace OrgChart.API.DataSources;

/// <summary>
/// Interface for organizational chart data sources
/// </summary>
public interface IOrgChartDataSource
{
    /// <summary>
    /// Retrieves organizational chart data from the configured source
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Organization chart data</returns>
    Task<OrgChartResponse> GetDataAsync(CancellationToken cancellationToken = default);
}