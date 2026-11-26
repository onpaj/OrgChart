using System.Security.Claims;

namespace OrgChart.API.Services;

/// <summary>
/// Decides what the current user may do with the org chart
/// </summary>
public interface IUserPermissionService
{
    /// <summary>True if the user may view the org chart.</summary>
    Task<bool> CanReadAsync(ClaimsPrincipal? user);

    /// <summary>True if the user may change the org chart.</summary>
    Task<bool> CanEditAsync(ClaimsPrincipal? user);
}
