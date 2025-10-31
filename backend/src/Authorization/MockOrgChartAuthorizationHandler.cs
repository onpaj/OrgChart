using Microsoft.AspNetCore.Authorization;

namespace OrgChart.API.Authorization;

/// <summary>
/// Mock authorization handler for org chart requirements - allows all operations
/// </summary>
public class MockOrgChartAuthorizationHandler : AuthorizationHandler<OrgChartRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OrgChartRequirement requirement)
    {
        // Allow all operations in mock mode
        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}