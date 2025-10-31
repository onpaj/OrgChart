using Microsoft.Extensions.Caching.Memory;
using OrgChart.API.Models;
using System.Collections.Concurrent;

namespace OrgChart.API.Services;

/// <summary>
/// Service for caching and background loading of user data from Microsoft Graph
/// </summary>
public class UserCacheService : IUserCacheService
{
    private readonly IMicrosoftGraphService _graphService;
    private readonly IOrgChartService _orgChartService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<UserCacheService> _logger;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _loadingSemaphores;
    
    // Cache keys
    private const string USER_PROFILE_KEY_PREFIX = "user_profile_";
    private const string USER_PHOTO_KEY_PREFIX = "user_photo_";
    private const string CACHE_STATS_KEY = "cache_stats";
    
    // Cache settings
    private readonly TimeSpan _profileCacheExpiry = TimeSpan.FromHours(6);
    private readonly TimeSpan _photoCacheExpiry = TimeSpan.FromHours(24);
    
    private readonly CacheStats _stats;

    public UserCacheService(
        IMicrosoftGraphService graphService,
        IOrgChartService orgChartService,
        IMemoryCache cache,
        ILogger<UserCacheService> logger)
    {
        _graphService = graphService;
        _orgChartService = orgChartService;
        _cache = cache;
        _logger = logger;
        _loadingSemaphores = new ConcurrentDictionary<string, SemaphoreSlim>();
        _stats = new CacheStats();
    }

    public async Task<GraphUserInfo?> GetUserAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var cacheKey = USER_PROFILE_KEY_PREFIX + email.ToLowerInvariant();
        
        // Try to get from cache first
        if (_cache.TryGetValue(cacheKey, out GraphUserInfo? cachedUser))
        {
            _stats.ProfileHits++;
            _logger.LogDebug("User profile cache hit for: {Email}", email);
            return cachedUser;
        }

        _stats.ProfileMisses++;
        _logger.LogDebug("User profile cache miss for: {Email}", email);

        // Use semaphore to prevent multiple concurrent requests for the same user
        var semaphore = _loadingSemaphores.GetOrAdd(email, _ => new SemaphoreSlim(1, 1));
        
        await semaphore.WaitAsync();
        try
        {
            // Check cache again in case another thread loaded it
            if (_cache.TryGetValue(cacheKey, out cachedUser))
            {
                _stats.ProfileHits++;
                return cachedUser;
            }

            // Load from Graph API
            var user = await _graphService.GetUserByEmailAsync(email);
            
            if (user != null)
            {
                _cache.Set(cacheKey, user, _profileCacheExpiry);
                _logger.LogInformation("Cached user profile for: {Email}", email);
            }
            
            return user;
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task<(string? photoData, string? contentType)> GetUserPhotoAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return (null, null);

        var cacheKey = USER_PHOTO_KEY_PREFIX + email.ToLowerInvariant();
        
        // Try to get from cache first
        if (_cache.TryGetValue(cacheKey, out (string? photoData, string? contentType) cachedPhoto))
        {
            _stats.PhotoHits++;
            _logger.LogDebug("User photo cache hit for: {Email}", email);
            return cachedPhoto;
        }

        _stats.PhotoMisses++;
        _logger.LogDebug("User photo cache miss for: {Email}", email);

        // Use semaphore to prevent multiple concurrent requests for the same photo
        var semaphore = _loadingSemaphores.GetOrAdd($"photo_{email}", _ => new SemaphoreSlim(1, 1));
        
        await semaphore.WaitAsync();
        try
        {
            // Check cache again in case another thread loaded it
            if (_cache.TryGetValue(cacheKey, out cachedPhoto))
            {
                _stats.PhotoHits++;
                return cachedPhoto;
            }

            // Load from Graph API
            var photo = await _graphService.GetUserPhotoAsync(email);
            
            // Cache even null results to avoid repeated failed requests
            _cache.Set(cacheKey, photo, _photoCacheExpiry);
            
            if (photo.photoData != null)
            {
                _logger.LogInformation("Cached user photo for: {Email}", email);
            }
            
            return photo;
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task PreloadAllUsersAsync()
    {
        try
        {
            _logger.LogInformation("Starting preload of all user data...");
            
            // Get all employees from org chart
            var orgChartResponse = await _orgChartService.GetOrganizationStructureAsync();
            var orgData = orgChartResponse.Organization;
            var allEmails = orgData.Positions
                .SelectMany(p => p.Employees)
                .Where(e => !string.IsNullOrWhiteSpace(e.Email))
                .Select(e => e.Email)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            _logger.LogInformation("Found {Count} unique emails to preload", allEmails.Count);

            // Load user profiles in parallel with concurrency limit
            var semaphore = new SemaphoreSlim(5); // Max 5 concurrent requests
            var tasks = allEmails.Select(async email =>
            {
                await semaphore.WaitAsync();
                try
                {
                    // Load profile (this will cache it)
                    await GetUserAsync(email);
                    
                    // Small delay to avoid overwhelming Graph API
                    await Task.Delay(100);
                    
                    // Load photo (this will cache it)
                    await GetUserPhotoAsync(email);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to preload user data for: {Email}", email);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);
            
            _logger.LogInformation("Completed preload of user data for {Count} users", allEmails.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user data preload");
        }
    }

    public async Task RefreshUserAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return;

        var profileKey = USER_PROFILE_KEY_PREFIX + email.ToLowerInvariant();
        var photoKey = USER_PHOTO_KEY_PREFIX + email.ToLowerInvariant();
        
        // Remove from cache
        _cache.Remove(profileKey);
        _cache.Remove(photoKey);
        
        _logger.LogInformation("Refreshing user data for: {Email}", email);
        
        // Reload data (this will cache it again)
        await GetUserAsync(email);
        await GetUserPhotoAsync(email);
    }

    public async Task<object> GetCacheStatsAsync()
    {
        await Task.CompletedTask;
        
        return new
        {
            ProfileStats = new
            {
                Hits = _stats.ProfileHits,
                Misses = _stats.ProfileMisses,
                HitRatio = _stats.ProfileHits + _stats.ProfileMisses > 0 
                    ? (double)_stats.ProfileHits / (_stats.ProfileHits + _stats.ProfileMisses) 
                    : 0
            },
            PhotoStats = new
            {
                Hits = _stats.PhotoHits,
                Misses = _stats.PhotoMisses,
                HitRatio = _stats.PhotoHits + _stats.PhotoMisses > 0 
                    ? (double)_stats.PhotoHits / (_stats.PhotoHits + _stats.PhotoMisses) 
                    : 0
            },
            LastUpdated = DateTime.UtcNow
        };
    }

    private class CacheStats
    {
        public long ProfileHits { get; set; }
        public long ProfileMisses { get; set; }
        public long PhotoHits { get; set; }
        public long PhotoMisses { get; set; }
    }
}