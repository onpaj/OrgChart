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

    /// <summary>
    /// User permissions for organizational chart operations
    /// </summary>
    public UserPermissions Permissions { get; set; } = new();
}

/// <summary>
/// User permissions for organizational chart operations
/// </summary>
public class UserPermissions
{
    /// <summary>
    /// Whether the user can edit (create, update, delete) positions and employees
    /// </summary>
    public bool CanEdit { get; set; }
}