namespace OrgChart.API.Models;

/// <summary>
/// Represents the complete organizational structure
/// </summary>
public class OrganizationData
{
    /// <summary>
    /// Organization name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// List of all positions in the organization
    /// </summary>
    public List<Position> Positions { get; set; } = new();
}