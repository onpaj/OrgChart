using System.Security.Claims;
using OrgChart.API.Authorization;

namespace OrgChart.API.Services;

/// <summary>
/// Real implementation of user permission service
/// </summary>
public class RoleBaseUserPermissionService : IUserPermissionService
{
    public bool CanEdit(ClaimsPrincipal? user)
    {
        // Check if user is authenticated and has admin role
        return user?.Identity?.IsAuthenticated == true && 
               user.HasClaim(OrgChartClaims.Types.Role, OrgChartClaims.Roles.Admin);
    }
}