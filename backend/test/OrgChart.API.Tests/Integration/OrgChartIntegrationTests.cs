using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using OrgChart.API.Models;
using OrgChart.API.Services;
using OrgChart.API.Tests.TestHelpers;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace OrgChart.API.Tests.Integration;

public class OrgChartIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public OrgChartIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Get_OrgChart_ReturnsSuccessAndCorrectContentType()
    {
        // Arrange
        var expectedResponse = CreateSampleOrgChartResponse();
        var jsonResponse = JsonSerializer.Serialize(expectedResponse);

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        var configuration = new Dictionary<string, string?>
        {
            ["Authentication:Enabled"] = "false",
            ["OrgChart:DataSourceUrl"] = "https://api.example.com/orgchart"
        };

        var factory = new TestWebApplicationFactory<Program>(
            mockHttpMessageHandler: mockHttpMessageHandler,
            configuration: configuration);

        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/orgchart");

        // Assert
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var content = await response.Content.ReadFromJsonAsync<OrgChartResponse>();
        content.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task Get_OrgChart_WithAuthenticationEnabled_AndNoAuth_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Authentication:Enabled"] = "true",
                    ["OrgChart:DataSourceUrl"] = "https://api.example.com/orgchart"
                });
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/api/orgchart");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_OrgChart_WhenExternalServiceFails_ReturnsInternalServerError()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Authentication:Enabled"] = "false",
                    ["OrgChart:DataSourceUrl"] = "https://api.example.com/orgchart"
                });
            });

            builder.ConfigureServices(services =>
            {
                var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
                
                mockHttpMessageHandler
                    .Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ThrowsAsync(new HttpRequestException("External service unavailable"));

                services.AddSingleton(mockHttpMessageHandler.Object);
                
                var serviceDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(HttpClient));
                if (serviceDescriptor != null)
                {
                    services.Remove(serviceDescriptor);
                }

                services.AddScoped<HttpClient>(provider => 
                    new HttpClient(provider.GetRequiredService<HttpMessageHandler>()));
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/api/orgchart");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        
        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("Failed to fetch organizational structure");
    }

    [Fact]
    public async Task Get_OrgChart_WithInvalidJson_ReturnsInternalServerError()
    {
        // Arrange
        var invalidJson = "{ invalid json }";

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(invalidJson, Encoding.UTF8, "application/json")
        };

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        var configuration = new Dictionary<string, string?>
        {
            ["Authentication:Enabled"] = "false",
            ["OrgChart:DataSourceUrl"] = "https://api.example.com/orgchart"
        };

        var factory = new TestWebApplicationFactory<Program>(
            mockHttpMessageHandler: mockHttpMessageHandler,
            configuration: configuration);

        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/orgchart");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        
        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("Failed to parse organizational structure");
    }

    [Fact]
    public async Task Get_OrgChart_WithCorsHeaders_ShouldIncludeCorsHeaders()
    {
        // Arrange
        var expectedResponse = CreateSampleOrgChartResponse();
        var jsonResponse = JsonSerializer.Serialize(expectedResponse);

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        var configuration = new Dictionary<string, string?>
        {
            ["Authentication:Enabled"] = "false",
            ["OrgChart:DataSourceUrl"] = "https://api.example.com/orgchart",
            ["Cors:AllowedOrigins:0"] = "http://localhost:3000"
        };

        var factory = new TestWebApplicationFactory<Program>(
            mockHttpMessageHandler: mockHttpMessageHandler,
            configuration: configuration);

        var client = factory.CreateClient();

        // Add Origin header to trigger CORS
        client.DefaultRequestHeaders.Add("Origin", "http://localhost:3000");

        // Act
        var response = await client.GetAsync("/api/orgchart");

        // Assert
        response.EnsureSuccessStatusCode();
        
        // CORS headers should be present when origin is allowed
        response.Headers.Should().ContainKey("Access-Control-Allow-Origin");
    }

    private static OrgChartResponse CreateSampleOrgChartResponse()
    {
        return new OrgChartResponse
        {
            Organization = new OrganizationData
            {
                Name = "Test Organization",
                Positions = new List<Position>
                {
                    new Position
                    {
                        Id = "pos1",
                        Title = "CEO",
                        Description = "Chief Executive Officer",
                        Level = 1,
                        Department = "Executive",
                        Employees = new List<Employee>
                        {
                            new Employee
                            {
                                Id = "emp1",
                                Name = "John Doe",
                                Email = "john.doe@company.com",
                                StartDate = "2020-01-01",
                                IsPrimary = true
                            }
                        }
                    },
                    new Position
                    {
                        Id = "pos2",
                        Title = "CTO",
                        Description = "Chief Technology Officer",
                        Level = 2,
                        ParentPositionId = "pos1",
                        Department = "Technology",
                        Employees = new List<Employee>
                        {
                            new Employee
                            {
                                Id = "emp2",
                                Name = "Jane Smith",
                                Email = "jane.smith@company.com",
                                StartDate = "2020-06-01",
                                IsPrimary = true
                            }
                        }
                    }
                }
            }
        };
    }
}