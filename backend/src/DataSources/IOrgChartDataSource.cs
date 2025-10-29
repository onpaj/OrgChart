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

    /// <summary>
    /// Retrieves organizational chart data from the configured source with forwarded headers
    /// </summary>
    /// <param name="forwardedHeaders">Headers to forward from the original request (e.g., Authorization)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Organization chart data</returns>
    Task<OrgChartResponse> GetDataAsync(Dictionary<string, string> forwardedHeaders, CancellationToken cancellationToken = default);
}