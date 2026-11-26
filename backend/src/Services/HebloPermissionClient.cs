using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OrgChart.API.Configuration;

namespace OrgChart.API.Services;

/// <summary>Asks Heblo which permissions the signed-in user holds.</summary>
public interface IHebloPermissionClient
{
    /// <summary>Returns the user's Heblo permissions, or <see cref="HebloPermissions.None"/> when
    /// Heblo cannot be asked (deny by default). Throws only when the caller cancels.</summary>
    Task<HebloPermissions> GetPermissionsAsync(string userObjectId, string userAccessToken, CancellationToken cancellationToken);
}

public sealed class HebloPermissionClient : IHebloPermissionClient
{
    private const string MeEndpoint = "api/auth/me";
    private const string CacheKeyPrefix = "heblo-perms:";
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _httpClient;
    private readonly IHebloTokenProvider _tokenProvider;
    private readonly IMemoryCache _cache;
    private readonly HebloOptions _options;
    private readonly ILogger<HebloPermissionClient> _logger;

    public HebloPermissionClient(
        HttpClient httpClient,
        IHebloTokenProvider tokenProvider,
        IMemoryCache cache,
        IOptions<HebloOptions> options,
        ILogger<HebloPermissionClient> logger)
    {
        _httpClient = httpClient;
        _tokenProvider = tokenProvider;
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<HebloPermissions> GetPermissionsAsync(
        string userObjectId, string userAccessToken, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeyPrefix + userObjectId;
        if (_cache.TryGetValue(cacheKey, out HebloPermissions? cached) && cached is not null)
        {
            return cached;
        }

        try
        {
            var permissions = await FetchAsync(userAccessToken, cancellationToken);
            _cache.Set(cacheKey, permissions, TimeSpan.FromMinutes(_options.CacheMinutes));
            return permissions;
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            // Failures are not cached, so access returns as soon as Heblo answers again.
            _logger.LogError(ex, "Could not read Heblo permissions for user {UserObjectId}; denying access", userObjectId);
            return HebloPermissions.None;
        }
    }

    private async Task<HebloPermissions> FetchAsync(string userAccessToken, CancellationToken cancellationToken)
    {
        var hebloToken = await _tokenProvider.AcquireTokenOnBehalfOfAsync(userAccessToken, cancellationToken);

        using var request = new HttpRequestMessage(HttpMethod.Get, BuildMeUri());
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", hebloToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Heblo {MeEndpoint} returned {(int)response.StatusCode}");
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var me = JsonSerializer.Deserialize<MeResponse>(body, JsonOptions)
            ?? throw new JsonException($"Heblo {MeEndpoint} returned an empty body");

        return new HebloPermissions(me.IsSuperUser, me.Permissions ?? new List<string>());
    }

    private Uri BuildMeUri() => new(new Uri(_options.BaseUrl.TrimEnd('/') + "/"), MeEndpoint);

    private sealed class MeResponse
    {
        public bool IsSuperUser { get; set; }
        public List<string>? Permissions { get; set; }
    }
}
