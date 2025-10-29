using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using OrgChart.API.Configuration;
using OrgChart.API.DataSources;
using OrgChart.API.Exceptions;
using OrgChart.API.Models;
using System.Net;
using System.Text;
using System.Text.Json;

namespace OrgChart.API.Tests.Unit.DataSources;

public class UrlBasedDataSourceTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly Mock<ILogger<UrlBasedDataSource>> _mockLogger;
    private readonly Mock<IOptions<OrgChartOptions>> _mockOptions;
    private readonly HttpClient _httpClient;
    private readonly UrlBasedDataSource _dataSource;
    private readonly OrgChartOptions _options;

    public UrlBasedDataSourceTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _mockLogger = new Mock<ILogger<UrlBasedDataSource>>();
        _mockOptions = new Mock<IOptions<OrgChartOptions>>();
        
        _options = new OrgChartOptions
        {
            DataSourceUrl = "https://api.example.com/orgchart"
        };
        
        _mockOptions.Setup(o => o.Value).Returns(_options);
        
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        _dataSource = new UrlBasedDataSource(_httpClient, _mockOptions.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetDataAsync_WhenHttpRequestSucceeds_ShouldReturnDeserializedData()
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
        var result = await _dataSource.GetDataAsync();

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
    public async Task GetDataAsync_WhenHttpRequestFails_ShouldThrowDataSourceException()
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
        var exception = await Assert.ThrowsAsync<DataSourceException>(
            () => _dataSource.GetDataAsync());
        
        exception.Message.Should().Contain("Failed to fetch organizational structure from URL");
        exception.InnerException.Should().BeOfType<HttpRequestException>();
    }

    [Fact]
    public async Task GetDataAsync_WhenHttpClientThrowsException_ShouldThrowDataSourceException()
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
        var exception = await Assert.ThrowsAsync<DataSourceException>(
            () => _dataSource.GetDataAsync());
        
        exception.Message.Should().Contain("Failed to fetch organizational structure from URL");
        exception.InnerException.Should().Be(httpException);
    }

    [Fact]
    public async Task GetDataAsync_WhenResponseIsInvalidJson_ShouldThrowDataSourceException()
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
        var exception = await Assert.ThrowsAsync<DataSourceException>(
            () => _dataSource.GetDataAsync());
        
        exception.Message.Should().Contain("Failed to parse organizational structure from URL");
        exception.InnerException.Should().BeOfType<JsonException>();
    }

    [Fact]
    public async Task GetDataAsync_WhenResponseIsNull_ShouldThrowDataSourceException()
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
        var exception = await Assert.ThrowsAsync<DataSourceException>(
            () => _dataSource.GetDataAsync());
        
        exception.Message.Should().Contain("Failed to deserialize organizational structure from URL data source");
    }

    [Fact]
    public async Task GetDataAsync_WhenSuccessful_ShouldLogInformationMessages()
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
        await _dataSource.GetDataAsync();

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Fetching organizational structure from URL data source")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully fetched organizational structure from URL data source")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetDataAsync_WhenHttpExceptionOccurs_ShouldLogError()
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
        await Assert.ThrowsAsync<DataSourceException>(
            () => _dataSource.GetDataAsync());

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("HTTP error while fetching organizational structure from URL")),
                httpException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetDataAsync_WhenJsonExceptionOccurs_ShouldLogError()
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
        await Assert.ThrowsAsync<DataSourceException>(
            () => _dataSource.GetDataAsync());

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("JSON deserialization error for organizational structure from URL")),
                It.IsAny<JsonException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetDataAsync_WithCancellationToken_ShouldPassTokenToHttpClient()
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
        await _dataSource.GetDataAsync(cancellationToken);

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
                        Department = "Executive",
                        Employees = new List<Employee>
                        {
                            new Employee
                            {
                                Id = "emp1",
                                Name = "John Doe",
                                Email = "john.doe@company.com",
                                StartDate = "2020-01-01"
                            }
                        }
                    },
                    new Position
                    {
                        Id = "pos2",
                        Title = "CTO",
                        Description = "Chief Technology Officer",
                        ParentPositionId = "pos1",
                        Department = "Technology",
                        Employees = new List<Employee>
                        {
                            new Employee
                            {
                                Id = "emp2",
                                Name = "Jane Smith",
                                Email = "jane.smith@company.com",
                                StartDate = "2020-06-01"
                            }
                        }
                    }
                }
            }
        };
    }
}