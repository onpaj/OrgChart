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
    /// User's home phone
    /// </summary>
    public string? HomePhone { get; set; }

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

    // Additional properties from MS Graph
    
    /// <summary>
    /// User's given name (first name)
    /// </summary>
    public string? GivenName { get; set; }

    /// <summary>
    /// User's surname (last name)
    /// </summary>
    public string? Surname { get; set; }

    /// <summary>
    /// User's company name
    /// </summary>
    public string? CompanyName { get; set; }

    /// <summary>
    /// User's employee ID
    /// </summary>
    public string? EmployeeId { get; set; }

    /// <summary>
    /// User's hire date
    /// </summary>
    public string? HireDate { get; set; }

    /// <summary>
    /// User's birthday
    /// </summary>
    public string? Birthday { get; set; }

    /// <summary>
    /// User's about me information
    /// </summary>
    public string? AboutMe { get; set; }

    /// <summary>
    /// User's interests
    /// </summary>
    public string[]? Interests { get; set; }

    /// <summary>
    /// User's skills
    /// </summary>
    public string[]? Skills { get; set; }

    /// <summary>
    /// User's responsibilities
    /// </summary>
    public string[]? Responsibilities { get; set; }

    /// <summary>
    /// User's manager information
    /// </summary>
    public ManagerInfo? Manager { get; set; }

    /// <summary>
    /// User's city
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// User's country
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// User's preferred language
    /// </summary>
    public string? PreferredLanguage { get; set; }

    /// <summary>
    /// User's usage location
    /// </summary>
    public string? UsageLocation { get; set; }
}

/// <summary>
/// Manager information from Microsoft Graph
/// </summary>
public class ManagerInfo
{
    /// <summary>
    /// Manager's display name
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Manager's email address
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Manager's job title
    /// </summary>
    public string? JobTitle { get; set; }
}