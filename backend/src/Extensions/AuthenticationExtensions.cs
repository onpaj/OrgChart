using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Options;

namespace OrgChart.API.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection ConfigureAuthentication(
        this IServiceCollection services, 
        WebApplicationBuilder builder, 
        ILogger logger)
    {
        var useMockAuth = builder.Configuration.GetValue<bool>("UseMockAuth", false);
        
        if (useMockAuth)
        {
            logger.LogInformation("Configuring Mock Authentication for development");
            ConfigureMockAuthentication(services);
        }
        else
        {
            logger.LogInformation("Configuring Real Authentication with Microsoft Identity");
            ConfigureRealAuthentication(services, builder);
        }
        
        return services;
    }

    private static void ConfigureRealAuthentication(
        IServiceCollection services, 
        WebApplicationBuilder builder)
    {
        // Configure manual JWT Bearer authentication for APIs ONLY
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                // Use the common endpoint for key discovery
                options.Authority = $"https://login.microsoftonline.com/{builder.Configuration["AzureAd:TenantId"]}/v2.0";
                options.Audience = builder.Configuration["AzureAd:Audience"]; // Use backend API audience
                
                options.TokenValidationParameters.ValidateIssuer = true;
                options.TokenValidationParameters.ValidateAudience = true; // Re-enable audience validation
                options.TokenValidationParameters.ValidateLifetime = true;
                options.TokenValidationParameters.ValidateIssuerSigningKey = true;
                options.TokenValidationParameters.ClockSkew = TimeSpan.FromMinutes(5);
                
                // Accept backend API audience
                options.TokenValidationParameters.ValidAudiences = new[] {
                    builder.Configuration["AzureAd:Audience"], // api://backend-client-id
                    builder.Configuration["AzureAd:ClientId"]  // backend-client-id
                };
                
                // Force refresh of signing keys
                options.TokenValidationParameters.RequireSignedTokens = true;
                
                // Log token validation issues
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                        Console.WriteLine($"Exception type: {context.Exception.GetType().Name}");
                        if (context.Exception.InnerException != null)
                        {
                            Console.WriteLine($"Inner exception: {context.Exception.InnerException.Message}");
                        }
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Console.WriteLine("Token validated successfully");
                        var claims = context.Principal?.Claims?.Select(c => $"{c.Type}: {c.Value}");
                        Console.WriteLine($"Claims: {string.Join(", ", claims ?? new string[0])}");
                        return Task.CompletedTask;
                    },
                    OnMessageReceived = context =>
                    {
                        Console.WriteLine($"Token received, length: {context.Token?.Length ?? 0}");
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        Console.WriteLine($"Challenge: {context.Error}, {context.ErrorDescription}");
                        return Task.CompletedTask;
                    }
                };
            });
    }

    private static void ConfigureMockAuthentication(IServiceCollection services)
    {
        services.AddAuthentication("Mock")
            .AddScheme<MockAuthenticationSchemeOptions, MockAuthenticationHandler>(
                "Mock", 
                options => { });
    }
}

public class MockAuthenticationSchemeOptions : AuthenticationSchemeOptions { }

public class MockAuthenticationHandler : AuthenticationHandler<MockAuthenticationSchemeOptions>
{
    public MockAuthenticationHandler(
        IOptionsMonitor<MockAuthenticationSchemeOptions> options,
        ILoggerFactory logger, 
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "mock-user-id"),
            new Claim(ClaimTypes.Name, "Mock User"),
            new Claim(ClaimTypes.Email, "mock@orgchart.com"),
            new Claim("oid", "00000000-0000-0000-0000-000000000000"), // Azure AD Object ID
            new Claim("tid", "11111111-1111-1111-1111-111111111111"), // Tenant ID
            new Claim(ClaimTypes.Role, "OrgChart_Reader"),
            new Claim(ClaimTypes.Role, "OrgChart_Writer"),
            new Claim("scp", "access_as_user"), // Scope claim
        };
        
        var identity = new ClaimsIdentity(claims, "Mock");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Mock");
        
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}