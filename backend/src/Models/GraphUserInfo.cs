namespace OrgChart.API.Models;

/// <summary>
/// Represents user information retrieved from Microsoft Graph
/// </summary>
public class GraphUserInfo
{
    /// <summary>
    /// User's display name
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// User's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's phone number
    /// </summary>
    public string? MobilePhone { get; set; }

    /// <summary>
    /// User's business phone
    /// </summary>
    public string? BusinessPhone { get; set; }

    /// <summary>
    /// User's job title
    /// </summary>
    public string? JobTitle { get; set; }

    /// <summary>
    /// User's department
    /// </summary>
    public string? Department { get; set; }

    /// <summary>
    /// Base64 encoded profile photo
    /// </summary>
    public string? ProfilePhoto { get; set; }

    /// <summary>
    /// Content type of the profile photo (e.g., image/jpeg)
    /// </summary>
    public string? PhotoContentType { get; set; }

    /// <summary>
    /// User's office location
    /// </summary>
    public string? OfficeLocation { get; set; }
}