using System.Security.Claims;

namespace OrgChart.API.Services;

/// <summary>
/// Service for checking user permissions
/// </summary>
public interface IUserPermissionService
{
    /// <summary>
    /// Checks if the current user can edit org chart data
    /// </summary>
    /// <param name="user">The user principal</param>
    /// <returns>True if user can edit, false otherwise</returns>
    bool CanEdit(ClaimsPrincipal? user);
}