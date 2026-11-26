namespace OrgChart.API.Services;

/// <summary>Heblo permission strings that govern the org chart.</summary>
public static class HebloPermissionNames
{
    public const string OrgChartRead = "anela.org_chart.read";
    public const string OrgChartWrite = "anela.org_chart.write";
}

/// <summary>What Heblo says a user may do (from GET /api/auth/me).</summary>
public sealed record HebloPermissions(bool IsSuperUser, IReadOnlyCollection<string> Permissions)
{
    public static readonly HebloPermissions None = new(false, Array.Empty<string>());

    public bool Has(string permission) =>
        IsSuperUser || Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
}
