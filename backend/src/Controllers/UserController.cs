using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OrgChart.API.Services;
using OrgChart.API.Models;
using OrgChart.API.Authorization;

namespace OrgChart.API.Controllers;

/// <summary>
/// Controller for user-related operations including Microsoft Graph integration
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserCacheService _userCacheService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserCacheService userCacheService, ILogger<UserController> logger)
    {
        _userCacheService = userCacheService;
        _logger = logger;
    }

    /// <summary>
    /// Get user information from Microsoft Graph by email address
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <returns>User information including profile photo</returns>
    [HttpGet("profile")]
    [Authorize(Policy = OrgChartPolicies.Read)]
    public async Task<ActionResult<GraphUserInfo>> GetUserProfile([FromQuery] string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest("Email address is required");
        }

        try
        {
            var userInfo = await _userCacheService.GetUserAsync(email);
            
            if (userInfo == null)
            {
                return NotFound($"User with email '{email}' not found in Microsoft Graph");
            }

            return Ok(userInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user profile for email: {Email}", email);
            return StatusCode(500, "An error occurred while retrieving user information");
        }
    }

    /// <summary>
    /// Get user profile photo from Microsoft Graph by email address
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <returns>Base64 encoded profile photo</returns>
    [HttpGet("photo")]
    [Authorize(Policy = OrgChartPolicies.Read)]
    public async Task<ActionResult<object>> GetUserPhoto([FromQuery] string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest("Email address is required");
        }

        try
        {
            var (photoData, contentType) = await _userCacheService.GetUserPhotoAsync(email);
            
            if (photoData == null)
            {
                return NotFound($"Profile photo not found for user with email '{email}'");
            }

            return Ok(new 
            { 
                photoData, 
                contentType,
                dataUrl = $"data:{contentType};base64,{photoData}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user photo for email: {Email}", email);
            return StatusCode(500, "An error occurred while retrieving user photo");
        }
    }

    /// <summary>
    /// Get multiple user profiles by email addresses (batch operation)
    /// </summary>
    /// <param name="emails">List of email addresses</param>
    /// <returns>Dictionary of email to user information</returns>
    [HttpPost("profiles/batch")]
    [Authorize(Policy = OrgChartPolicies.Read)]
    public async Task<ActionResult<Dictionary<string, GraphUserInfo?>>> GetUserProfilesBatch([FromBody] string[] emails)
    {
        if (emails == null || emails.Length == 0)
        {
            return BadRequest("Email addresses are required");
        }

        if (emails.Length > 50) // Limit batch size to prevent abuse
        {
            return BadRequest("Maximum 50 email addresses allowed per batch request");
        }

        try
        {
            var results = new Dictionary<string, GraphUserInfo?>();
            
            // Process requests in parallel with some concurrency limit
            var semaphore = new SemaphoreSlim(10); // Max 10 concurrent requests
            var tasks = emails.Select(async email =>
            {
                await semaphore.WaitAsync();
                try
                {
                    var userInfo = await _userCacheService.GetUserAsync(email);
                    return new { Email = email, UserInfo = userInfo };
                }
                finally
                {
                    semaphore.Release();
                }
            });

            var batchResults = await Task.WhenAll(tasks);
            
            foreach (var result in batchResults)
            {
                results[result.Email] = result.UserInfo;
            }

            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user profiles for batch request");
            return StatusCode(500, "An error occurred while retrieving user information");
        }
    }

    /// <summary>
    /// Refresh user data in cache by email address
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <returns>Success confirmation</returns>
    [HttpPost("refresh")]
    [Authorize(Policy = OrgChartPolicies.Write)]
    public async Task<ActionResult> RefreshUser([FromQuery] string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest("Email address is required");
        }

        try
        {
            await _userCacheService.RefreshUserAsync(email);
            return Ok(new { message = $"User data refreshed for {email}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing user data for email: {Email}", email);
            return StatusCode(500, "An error occurred while refreshing user data");
        }
    }

    /// <summary>
    /// Trigger manual preload of all user data
    /// </summary>
    /// <returns>Success confirmation</returns>
    [HttpPost("preload")]
    [Authorize(Policy = OrgChartPolicies.Write)]
    public ActionResult PreloadAllUsers()
    {
        try
        {
            // Run preload in background to avoid request timeout
            _ = Task.Run(async () =>
            {
                try
                {
                    await _userCacheService.PreloadAllUsersAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during manual preload");
                }
            });

            return Ok(new { message = "User data preload started in background" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting user data preload");
            return StatusCode(500, "An error occurred while starting preload");
        }
    }

    /// <summary>
    /// Get cache statistics
    /// </summary>
    /// <returns>Cache performance statistics</returns>
    [HttpGet("cache/stats")]
    [Authorize(Policy = OrgChartPolicies.Read)]
    public async Task<ActionResult<object>> GetCacheStats()
    {
        try
        {
            var stats = await _userCacheService.GetCacheStatsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cache statistics");
            return StatusCode(500, "An error occurred while retrieving cache statistics");
        }
    }
}