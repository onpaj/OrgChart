namespace OrgChart.API.Authorization;

/// <summary>
/// Constants for claim types and values
/// </summary>
public static class OrgChartClaims
{
    /// <summary>
    /// Claim types used in authorization
    /// </summary>
    public static class Types
    {
        public const string Role = "role";
        public const string Scope = "scp";
    }

    /// <summary>
    /// Claim values for roles
    /// </summary>
    public static class Roles
    {
        public const string Admin = "OrgChart_Admin";
    }

    /// <summary>
    /// Claim values for scopes
    /// </summary>
    public static class Scopes
    {
        public const string AccessAsUser = "access_as_user";
    }
}

/// <summary>
/// Authorization policy names
/// </summary>
public static class OrgChartPolicies
{
    public const string Read = "OrgChart.Read";
    public const string Write = "OrgChart.Write";
}