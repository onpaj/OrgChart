using System.Security.Claims;

namespace OrgChart.API.Services;

/// <summary>
/// Mock implementation of user permission service for development - allows everything
/// </summary>
public class MockUserPermissionService : IUserPermissionService
{
    public Task<bool> CanReadAsync(ClaimsPrincipal? user) => Task.FromResult(true);

    public Task<bool> CanEditAsync(ClaimsPrincipal? user) => Task.FromResult(true);
}
