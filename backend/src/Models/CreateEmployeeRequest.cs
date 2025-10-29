namespace OrgChart.API.Models;

/// <summary>
/// Request model for creating a new employee
/// </summary>
public class CreateEmployeeRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string StartDate { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string PositionId { get; set; } = string.Empty;
}