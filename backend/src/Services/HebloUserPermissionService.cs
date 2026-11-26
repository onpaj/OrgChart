using System.Security.Claims;

namespace OrgChart.API.Services;

/// <summary>
/// Grants org chart access from the user's Heblo permissions:
/// read = anela.org_chart.read, edit = anela.org_chart.write, Heblo super_user = both.
/// </summary>
public class HebloUserPermissionService : IUserPermissionService
{
    private const string ObjectIdClaim = "oid";
    private const string MappedObjectIdClaim = "http://schemas.microsoft.com/identity/claims/objectidentifier";
    private const string BearerPrefix = "Bearer ";

    private readonly IHebloPermissionClient _client;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HebloUserPermissionService(IHebloPermissionClient client, IHttpContextAccessor httpContextAccessor)
    {
        _client = client;
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<bool> CanReadAsync(ClaimsPrincipal? user) => HasAsync(user, HebloPermissionNames.OrgChartRead);

    public Task<bool> CanEditAsync(ClaimsPrincipal? user) => HasAsync(user, HebloPermissionNames.OrgChartWrite);

    private async Task<bool> HasAsync(ClaimsPrincipal? user, string permission)
    {
        if (user?.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        var objectId = user.FindFirst(ObjectIdClaim)?.Value ?? user.FindFirst(MappedObjectIdClaim)?.Value;
        var accessToken = GetBearerToken();
        if (string.IsNullOrWhiteSpace(objectId) || string.IsNullOrWhiteSpace(accessToken))
        {
            return false;
        }

        var cancellationToken = _httpContextAccessor.HttpContext?.RequestAborted ?? CancellationToken.None;
        var permissions = await _client.GetPermissionsAsync(objectId, accessToken, cancellationToken);
        return permissions.Has(permission);
    }

    private string? GetBearerToken()
    {
        var header = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
        if (string.IsNullOrEmpty(header) || !header.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return header[BearerPrefix.Length..].Trim();
    }
}
