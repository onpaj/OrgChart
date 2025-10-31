using System.Security.Claims;

namespace OrgChart.API.Services;

/// <summary>
/// Mock implementation of user permission service for development
/// </summary>
public class MockUserPermissionService : IUserPermissionService
{
    public bool CanEdit(ClaimsPrincipal? user)
    {
        return true;
    }
}