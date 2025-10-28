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

/// <summary>
/// Represents a position in the organizational structure
/// </summary>
public class Position
{
    /// <summary>
    /// Unique identifier for the position
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Position title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Position description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Hierarchical level (1 = top level, higher numbers = lower in hierarchy)
    /// </summary>
    public int? Level { get; set; }

    /// <summary>
    /// ID of the parent position in the hierarchy
    /// </summary>
    public string? ParentPositionId { get; set; }

    /// <summary>
    /// Department name
    /// </summary>
    public string Department { get; set; } = string.Empty;

    /// <summary>
    /// List of employees in this position
    /// </summary>
    public List<Employee> Employees { get; set; } = new();

    /// <summary>
    /// Optional URL to position description (e.g., SharePoint document)
    /// </summary>
    public string? Url { get; set; }
}

/// <summary>
/// Represents an employee in the organization
/// </summary>
public class Employee
{
    /// <summary>
    /// Unique identifier for the employee
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Employee full name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Employee email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Date when employee started in this position
    /// </summary>
    public string StartDate { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if this is the primary employee for the position
    /// </summary>
    public bool IsPrimary { get; set; }

    /// <summary>
    /// Optional URL to employee profile (e.g., MS Entra profile)
    /// </summary>
    public string? Url { get; set; }
}