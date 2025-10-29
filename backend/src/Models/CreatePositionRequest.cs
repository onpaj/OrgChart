namespace OrgChart.API.Models;

/// <summary>
/// Request model for creating a new position
/// </summary>
public class CreatePositionRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ParentPositionId { get; set; }
    public string Department { get; set; } = string.Empty;
    public string? Url { get; set; }
}