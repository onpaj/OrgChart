using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using OrgChart.API.Models;

namespace OrgChart.API.Services;

/// <summary>
/// Service for interacting with Microsoft Graph API
/// </summary>
public class MicrosoftGraphService : IMicrosoftGraphService
{
    private readonly GraphServiceClient _graphServiceClient;
    private readonly ILogger<MicrosoftGraphService> _logger;

    public MicrosoftGraphService(IConfiguration configuration, ILogger<MicrosoftGraphService> logger)
    {
        _logger = logger;
        
        var tenantId = configuration["AzureAd:TenantId"];
        var clientId = configuration["AzureAd:ClientId"];
        var clientSecret = configuration["AzureAd:ClientSecret"];

        if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            throw new InvalidOperationException("Azure AD configuration is missing. Please configure TenantId, ClientId, and ClientSecret.");
        }

        var options = new ClientSecretCredentialOptions
        {
            AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
        };

        var clientSecretCredential = new ClientSecretCredential(tenantId, clientId, clientSecret, options);

        _graphServiceClient = new GraphServiceClient(clientSecretCredential);
    }

    /// <summary>
    /// Get user information by email address
    /// </summary>
    public async Task<GraphUserInfo?> GetUserByEmailAsync(string email)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("Email address is empty or null");
                return null;
            }

            _logger.LogInformation("Fetching user information for email: {Email}", email);

            // Search for user by email - use basic properties first
            var users = await _graphServiceClient.Users
                .GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Filter = $"mail eq '{email}' or userPrincipalName eq '{email}'";
                    requestConfiguration.QueryParameters.Select = new[] { 
                        "id",
                        "displayName", 
                        "mail", 
                        "mobilePhone", 
                        "businessPhones",
                        "jobTitle", 
                        "department", 
                        "officeLocation",
                        "userPrincipalName",
                        "givenName",
                        "surname",
                        "companyName",
                        "employeeId",
                        "city",
                        "country",
                        "preferredLanguage",
                        "usageLocation"
                    };
                    requestConfiguration.QueryParameters.Expand = new[] { "manager($select=displayName,mail,jobTitle)" };
                });

            var user = users?.Value?.FirstOrDefault();
            if (user == null)
            {
                _logger.LogInformation("User not found for email: {Email}", email);
                return null;
            }

            // Get user photo
            var (photoData, contentType) = await GetUserPhotoAsync(email);

            // Try to get additional properties that require separate calls
            string? hireDate = null;
            string? birthday = null;
            string? aboutMe = null;
            string[]? interests = null;
            string[]? skills = null;
            string[]? responsibilities = null;

            try
            {
                // Get user with additional properties (these may fail, so we handle separately)
                var userDetailed = await _graphServiceClient.Users[user.Id]
                    .GetAsync(requestConfiguration =>
                    {
                        requestConfiguration.QueryParameters.Select = new[] { 
                            "hireDate",
                            "birthday",
                            "aboutMe",
                            "interests",
                            "skills",
                            "responsibilities"
                        };
                    });

                hireDate = userDetailed?.HireDate?.ToString("yyyy-MM-dd");
                birthday = userDetailed?.Birthday?.ToString("yyyy-MM-dd");
                aboutMe = userDetailed?.AboutMe;
                interests = userDetailed?.Interests?.ToArray();
                skills = userDetailed?.Skills?.ToArray();
                responsibilities = userDetailed?.Responsibilities?.ToArray();
            }
            catch (ServiceException ex) when (ex.ResponseStatusCode == 400 || ex.ResponseStatusCode == 403)
            {
                _logger.LogWarning("Additional user properties not available for {Email}: {Message}", email, ex.Message);
                // Continue without these properties - they may not be available for this user/tenant
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not fetch additional user properties for {Email}", email);
                // Continue without these properties
            }

            var graphUserInfo = new GraphUserInfo
            {
                DisplayName = user.DisplayName ?? string.Empty,
                Email = user.Mail ?? user.UserPrincipalName ?? email,
                MobilePhone = user.MobilePhone,
                BusinessPhone = user.BusinessPhones?.FirstOrDefault(),
                // HomePhone = user.HomePhone, // HomePhone is not available in MS Graph User object
                JobTitle = user.JobTitle,
                Department = user.Department,
                OfficeLocation = user.OfficeLocation,
                ProfilePhoto = photoData,
                PhotoContentType = contentType,
                // Additional properties
                GivenName = user.GivenName,
                Surname = user.Surname,
                CompanyName = user.CompanyName,
                EmployeeId = user.EmployeeId,
                HireDate = hireDate,
                Birthday = birthday,
                AboutMe = aboutMe,
                Interests = interests,
                Skills = skills,
                Responsibilities = responsibilities,
                City = user.City,
                Country = user.Country,
                PreferredLanguage = user.PreferredLanguage,
                UsageLocation = user.UsageLocation,
                Manager = user.Manager is Microsoft.Graph.Models.User manager ? new ManagerInfo
                {
                    DisplayName = manager.DisplayName,
                    Email = manager.Mail,
                    JobTitle = manager.JobTitle
                } : null
            };

            _logger.LogInformation("Successfully retrieved user information for: {Email}", email);
            return graphUserInfo;
        }
        catch (ServiceException ex)
        {
            _logger.LogError(ex, "Microsoft Graph API error when fetching user {Email}: {Message}", email, ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error when fetching user information for {Email}", email);
            return null;
        }
    }

    /// <summary>
    /// Get user profile photo by email address
    /// </summary>
    public async Task<(string? photoData, string? contentType)> GetUserPhotoAsync(string email)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return (null, null);
            }

            // First, find the user by email
            var users = await _graphServiceClient.Users
                .GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Filter = $"mail eq '{email}' or userPrincipalName eq '{email}'";
                    requestConfiguration.QueryParameters.Select = new[] { "id" };
                });

            var user = users?.Value?.FirstOrDefault();
            if (user?.Id == null)
            {
                _logger.LogInformation("User not found when fetching photo for email: {Email}", email);
                return (null, null);
            }

            // Get the user's photo
            var photoStream = await _graphServiceClient.Users[user.Id].Photo.Content.GetAsync();
            
            if (photoStream == null)
            {
                _logger.LogInformation("No photo found for user: {Email}", email);
                return (null, null);
            }

            using var memoryStream = new MemoryStream();
            await photoStream.CopyToAsync(memoryStream);
            var photoBytes = memoryStream.ToArray();
            
            if (photoBytes.Length == 0)
            {
                return (null, null);
            }

            var base64Photo = Convert.ToBase64String(photoBytes);
            
            // Try to get photo metadata for content type
            try
            {
                var photoMetadata = await _graphServiceClient.Users[user.Id].Photo.GetAsync();
                var contentType = photoMetadata?.AdditionalData?.ContainsKey("@odata.mediaContentType") == true
                    ? photoMetadata.AdditionalData["@odata.mediaContentType"]?.ToString()
                    : "image/jpeg"; // Default to JPEG

                return (base64Photo, contentType);
            }
            catch
            {
                // If we can't get metadata, assume JPEG
                return (base64Photo, "image/jpeg");
            }
        }
        catch (ServiceException ex) when (ex.ResponseStatusCode == (int)System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogInformation("No photo available for user: {Email}", email);
            return (null, null);
        }
        catch (ServiceException ex)
        {
            _logger.LogError(ex, "Microsoft Graph API error when fetching photo for {Email}: {Message}", email, ex.Message);
            return (null, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error when fetching photo for {Email}", email);
            return (null, null);
        }
    }
}