using OrgChart.API.Models;

namespace OrgChart.API.Services;

/// <summary>
/// Service for interacting with Microsoft Graph API
/// </summary>
public interface IMicrosoftGraphService
{
    /// <summary>
    /// Get user information by email address
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <returns>User information from Microsoft Graph or null if not found</returns>
    Task<GraphUserInfo?> GetUserByEmailAsync(string email);

    /// <summary>
    /// Get user profile photo by email address
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <returns>Base64 encoded photo data or null if not found</returns>
    Task<(string? photoData, string? contentType)> GetUserPhotoAsync(string email);
}