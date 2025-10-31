using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using OrgChart.API.DataSources;
using OrgChart.API.Repositories;
using OrgChart.API.Services;
using OrgChart.API.Authorization;
using System.Net;

namespace OrgChart.API.Tests.TestHelpers;

public class TestWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    private readonly Mock<IOrgChartService>? _mockOrgChartService;
    private readonly Mock<HttpMessageHandler>? _mockHttpMessageHandler;
    private readonly Dictionary<string, string?> _configuration;

    public TestWebApplicationFactory(
        Mock<IOrgChartService>? mockOrgChartService = null,
        Mock<HttpMessageHandler>? mockHttpMessageHandler = null,
        Dictionary<string, string?>? configuration = null)
    {
        _mockOrgChartService = mockOrgChartService;
        _mockHttpMessageHandler = mockHttpMessageHandler;
        _configuration = configuration ?? new Dictionary<string, string?>();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Add test configuration
            var testConfig = new Dictionary<string, string?>
            {
                ["UseMockAuth"] = "true",
                ["OrgChart:DataSourceType"] = "Url",
                ["OrgChart:DataSourceUrl"] = "https://test.example.com/orgchart",
                ["OrgChart:Permissions:InsertEnabled"] = "false",
                ["OrgChart:Permissions:UpdateEnabled"] = "false",
                ["OrgChart:Permissions:DeleteEnabled"] = "false"
            };

            // Merge with provided configuration
            foreach (var kvp in _configuration)
            {
                testConfig[kvp.Key] = kvp.Value;
            }

            config.AddInMemoryCollection(testConfig);
        });

        builder.ConfigureServices(services =>
        {
            // Check if mock auth is enabled
            var useMockAuth = _configuration.GetValueOrDefault("UseMockAuth", "false");
            
            if (useMockAuth == "true")
            {
                // Replace authorization policies when using mock auth to allow all requests
                services.AddAuthorization(options =>
                {
                    // Remove the fallback policy that requires authentication
                    options.FallbackPolicy = null;
                    
                    // Create permissive policies for testing
                    options.AddPolicy(OrgChartPolicies.Read, policy => 
                        policy.RequireAssertion(_ => true));
                        
                    options.AddPolicy(OrgChartPolicies.Write, policy => 
                        policy.RequireAssertion(_ => true));
                });

                // Remove existing permission service and replace with mock
                var permissionServiceDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IUserPermissionService));
                if (permissionServiceDescriptor != null)
                {
                    services.Remove(permissionServiceDescriptor);
                }
                services.AddScoped<IUserPermissionService, MockUserPermissionService>();
            }
            
            // Replace the OrgChartService with mock if provided
            if (_mockOrgChartService != null)
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IOrgChartService));
                
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddSingleton(_mockOrgChartService.Object);
            }
            // Replace HttpClient with mock handler if provided
            else if (_mockHttpMessageHandler != null)
            {
                // Remove existing HttpClient registrations
                var httpClientDescriptors = services.Where(d => 
                    d.ServiceType.Name.Contains("HttpClient") ||
                    d.ImplementationType?.Name.Contains("HttpClient") == true).ToList();
                
                foreach (var descriptor in httpClientDescriptors)
                {
                    services.Remove(descriptor);
                }

                // Add HttpClient with mock handler for the data source
                services.AddHttpClient<IOrgChartDataSource, UrlBasedDataSource>()
                    .ConfigurePrimaryHttpMessageHandler(() => _mockHttpMessageHandler.Object);
                
                // Add the repository and service
                services.AddScoped<IOrgChartRepository, UrlBasedRepository>();
                services.AddScoped<IOrgChartService, OrgChartService>();
            }

            // Reduce logging noise in tests
            services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Warning));
        });

        builder.UseEnvironment("Testing");
    }
}