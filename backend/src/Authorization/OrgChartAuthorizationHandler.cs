using Microsoft.AspNetCore.Authorization;
using OrgChart.API.Services;

namespace OrgChart.API.Authorization;

/// <summary>
/// Authorization handler for org chart requirements; access comes from Heblo permissions
/// via <see cref="IUserPermissionService"/>.
/// </summary>
public class OrgChartAuthorizationHandler : AuthorizationHandler<OrgChartRequirement>
{
    private readonly IUserPermissionService _permissions;

    public OrgChartAuthorizationHandler(IUserPermissionService permissions)
    {
        _permissions = permissions;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OrgChartRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var isAllowed = requirement.AccessLevel switch
        {
            OrgChartAccessLevel.Read => await _permissions.CanReadAsync(context.User),
            OrgChartAccessLevel.Write => await _permissions.CanEditAsync(context.User),
            _ => false,
        };

        if (isAllowed)
        {
            context.Succeed(requirement);
        }
    }
}
