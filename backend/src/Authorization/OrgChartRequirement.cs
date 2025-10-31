using Microsoft.AspNetCore.Authorization;

namespace OrgChart.API.Authorization;

/// <summary>
/// Authorization requirement for org chart operations
/// </summary>
public class OrgChartRequirement : IAuthorizationRequirement
{
    public OrgChartAccessLevel AccessLevel { get; }

    public OrgChartRequirement(OrgChartAccessLevel accessLevel)
    {
        AccessLevel = accessLevel;
    }
}