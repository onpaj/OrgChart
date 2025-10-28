using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using OrgChart.API.DataSources;
using OrgChart.API.Services;
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
                ["Authentication:Enabled"] = "false",
                ["OrgChart:DataSourceType"] = "Url",
                ["OrgChart:DataSourceUrl"] = "https://test.example.com/orgchart"
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
                
                // Add the service
                services.AddScoped<IOrgChartService, OrgChartService>();
            }

            // Reduce logging noise in tests
            services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Warning));
        });

        builder.UseEnvironment("Testing");
    }
}