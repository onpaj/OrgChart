using OrgChart.API.Models;

namespace OrgChart.API.Services;

/// <summary>
/// Service for caching and background loading of user data from Microsoft Graph
/// </summary>
public interface IUserCacheService
{
    /// <summary>
    /// Get user information from cache, with fallback to Graph API if not cached
    /// </summary>
    Task<GraphUserInfo?> GetUserAsync(string email);

    /// <summary>
    /// Get user photo from cache, with fallback to Graph API if not cached
    /// </summary>
    Task<(string? photoData, string? contentType)> GetUserPhotoAsync(string email);

    /// <summary>
    /// Preload user data for all employees in the organization
    /// </summary>
    Task PreloadAllUsersAsync();

    /// <summary>
    /// Force refresh of specific user data
    /// </summary>
    Task RefreshUserAsync(string email);

    /// <summary>
    /// Get cache statistics
    /// </summary>
    Task<object> GetCacheStatsAsync();
}