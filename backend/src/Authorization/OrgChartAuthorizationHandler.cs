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
        if (!context.User.Identity?.IsAuthenticated == true)
        {
            return Task.CompletedTask;
        }

        // Check access level requirements
        switch (requirement.AccessLevel)
        {
            case OrgChartAccessLevel.Read:
                // For read access, require scope claim
                if (context.User.HasClaim(OrgChartClaims.Types.Scope, OrgChartClaims.Scopes.AccessAsUser))
                {
                    context.Succeed(requirement);
                }
                break;

            case OrgChartAccessLevel.Write:
                // For write access, require admin role
                if (context.User.HasClaim(OrgChartClaims.Types.Role, OrgChartClaims.Roles.Admin))
                {
                    context.Succeed(requirement);
                }
                break;
        }

        return Task.CompletedTask;
    }
}