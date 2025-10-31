using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace OrgChart.API.Authorization;

/// <summary>
/// Real authorization handler for org chart requirements
/// </summary>
public class OrgChartAuthorizationHandler : AuthorizationHandler<OrgChartRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OrgChartRequirement requirement)
    {
        // Check if user is authenticated
        if (context.User.Identity?.IsAuthenticated == false)
        {
            return Task.CompletedTask;
        }

        // Check access level requirements
        switch (requirement.AccessLevel)
        {
            case OrgChartAccessLevel.Read:
                // For read access, authenticated user is enough
                context.Succeed(requirement);
                break;

            case OrgChartAccessLevel.Write:
                // For write access, require admin role
                if (context.User.HasClaim(ClaimTypes.Role, OrgChartClaims.Roles.Admin))
                {
                    context.Succeed(requirement);
                }
                break;
        }

        return Task.CompletedTask;
    }
}