namespace OrgChart.API.Models;

/// <summary>
/// Root response wrapper for organizational chart data
/// </summary>
public class OrgChartResponse
{
    /// <summary>
    /// The complete organizational structure
    /// </summary>
    public OrganizationData Organization { get; set; } = new();
}