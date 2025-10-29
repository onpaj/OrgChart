namespace OrgChart.API.Models;

/// <summary>
/// Request model for updating an existing employee
/// </summary>
public class UpdateEmployeeRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string StartDate { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public string? Url { get; set; }
    public string PositionId { get; set; } = string.Empty;
}