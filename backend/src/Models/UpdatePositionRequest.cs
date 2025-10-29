namespace OrgChart.API.Models;

/// <summary>
/// Request model for updating an existing position
/// </summary>
public class UpdatePositionRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? Level { get; set; }
    public string? ParentPositionId { get; set; }
    public string Department { get; set; } = string.Empty;
    public string? Url { get; set; }
}