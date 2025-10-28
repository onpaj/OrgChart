using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using OrgChart.API.Configuration;
using OrgChart.API.Models;
using OrgChart.API.Services;
using System.Net;
using System.Text;
using System.Text.Json;

namespace OrgChart.API.Tests.Unit.Services;

public class OrgChartServiceTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly Mock<ILogger<OrgChartService>> _mockLogger;
    private readonly Mock<IOptions<OrgChartOptions>> _mockOptions;
    private readonly HttpClient _httpClient;
    private readonly OrgChartService _service;
    private readonly OrgChartOptions _options;

    public OrgChartServiceTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _mockLogger = new Mock<ILogger<OrgChartService>>();
        _mockOptions = new Mock<IOptions<OrgChartOptions>>();
        
        _options = new OrgChartOptions
        {
            DataSourceUrl = "https://api.example.com/orgchart"
        };
        
        _mockOptions.Setup(o => o.Value).Returns(_options);
        
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        _service = new OrgChartService(_httpClient, _mockOptions.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetOrganizationStructureAsync_WhenHttpRequestSucceeds_ShouldReturnDeserializedData()
    {
        // Arrange
        var expectedResponse = CreateSampleOrgChartResponse();
        var jsonResponse = JsonSerializer.Serialize(expectedResponse);
        
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.GetOrganizationStructureAsync();

        // Assert
        result.Should().BeEquivalentTo(expectedResponse);
        
        _mockHttpMessageHandler
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Get && 
                    req.RequestUri!.ToString() == _options.DataSourceUrl),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetOrganizationStructureAsync_WhenHttpRequestFails_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetOrganizationStructureAsync());
        
        exception.Message.Should().Contain("Failed to fetch organizational structure");
        exception.InnerException.Should().BeOfType<HttpRequestException>();
    }

    [Fact]
    public async Task GetOrganizationStructureAsync_WhenHttpClientThrowsException_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var httpException = new HttpRequestException("Network error");
        
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(httpException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetOrganizationStructureAsync());
        
        exception.Message.Should().Contain("Failed to fetch organizational structure");
        exception.InnerException.Should().Be(httpException);
    }

    [Fact]
    public async Task GetOrganizationStructureAsync_WhenResponseIsInvalidJson_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var invalidJson = "{ invalid json }";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(invalidJson, Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetOrganizationStructureAsync());
        
        exception.Message.Should().Contain("Failed to parse organizational structure");
        exception.InnerException.Should().BeOfType<JsonException>();
    }

    [Fact]
    public async Task GetOrganizationStructureAsync_WhenResponseIsNull_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var nullJson = "null";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(nullJson, Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetOrganizationStructureAsync());
        
        exception.Message.Should().Contain("Failed to deserialize organizational structure");
    }

    [Fact]
    public async Task GetOrganizationStructureAsync_WhenSuccessful_ShouldLogInformationMessages()
    {
        // Arrange
        var expectedResponse = CreateSampleOrgChartResponse();
        var jsonResponse = JsonSerializer.Serialize(expectedResponse);
        
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        await _service.GetOrganizationStructureAsync();

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Fetching organizational structure from")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully loaded organizational structure")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetOrganizationStructureAsync_WhenHttpExceptionOccurs_ShouldLogError()
    {
        // Arrange
        var httpException = new HttpRequestException("Network error");
        
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(httpException);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetOrganizationStructureAsync());

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("HTTP error while fetching organizational structure")),
                httpException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetOrganizationStructureAsync_WithCancellationToken_ShouldPassTokenToHttpClient()
    {
        // Arrange
        var expectedResponse = CreateSampleOrgChartResponse();
        var jsonResponse = JsonSerializer.Serialize(expectedResponse);
        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;
        
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        await _service.GetOrganizationStructureAsync(cancellationToken);

        // Assert
        _mockHttpMessageHandler
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
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