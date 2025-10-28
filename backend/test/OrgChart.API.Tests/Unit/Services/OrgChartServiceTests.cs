using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OrgChart.API.DataSources;
using OrgChart.API.Exceptions;
using OrgChart.API.Models;
using OrgChart.API.Services;

namespace OrgChart.API.Tests.Unit.Services;

public class OrgChartServiceTests
{
    private readonly Mock<IOrgChartDataSource> _mockDataSource;
    private readonly Mock<ILogger<OrgChartService>> _mockLogger;
    private readonly OrgChartService _service;

    public OrgChartServiceTests()
    {
        _mockDataSource = new Mock<IOrgChartDataSource>();
        _mockLogger = new Mock<ILogger<OrgChartService>>();
        
        _service = new OrgChartService(_mockDataSource.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetOrganizationStructureAsync_WhenDataSourceSucceeds_ShouldReturnData()
    {
        // Arrange
        var expectedResponse = CreateSampleOrgChartResponse();
        
        _mockDataSource
            .Setup(ds => ds.GetDataAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _service.GetOrganizationStructureAsync();

        // Assert
        result.Should().BeEquivalentTo(expectedResponse);
        
        _mockDataSource.Verify(
            ds => ds.GetDataAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetOrganizationStructureAsync_WhenDataSourceFails_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var dataSourceException = new DataSourceException("Data source error", new Exception("Inner error"));

        _mockDataSource
            .Setup(ds => ds.GetDataAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(dataSourceException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetOrganizationStructureAsync());
        
        exception.Message.Should().Contain("Failed to retrieve organizational structure");
        exception.InnerException.Should().Be(dataSourceException);
    }

    [Fact]
    public async Task GetOrganizationStructureAsync_WhenUnexpectedExceptionOccurs_ShouldRethrow()
    {
        // Arrange
        var unexpectedException = new InvalidOperationException("Unexpected error");
        
        _mockDataSource
            .Setup(ds => ds.GetDataAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(unexpectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetOrganizationStructureAsync());
        
        exception.Should().Be(unexpectedException);
    }

    [Fact]
    public async Task GetOrganizationStructureAsync_WhenSuccessful_ShouldLogInformationMessages()
    {
        // Arrange
        var expectedResponse = CreateSampleOrgChartResponse();
        
        _mockDataSource
            .Setup(ds => ds.GetDataAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        await _service.GetOrganizationStructureAsync();

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Retrieving organizational structure from data source")),
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
    public async Task GetOrganizationStructureAsync_WhenDataSourceExceptionOccurs_ShouldLogError()
    {
        // Arrange
        var dataSourceException = new DataSourceException("Data source error");
        
        _mockDataSource
            .Setup(ds => ds.GetDataAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(dataSourceException);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetOrganizationStructureAsync());

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Data source error while retrieving organizational structure")),
                dataSourceException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetOrganizationStructureAsync_WithCancellationToken_ShouldPassTokenToDataSource()
    {
        // Arrange
        var expectedResponse = CreateSampleOrgChartResponse();
        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;
        
        _mockDataSource
            .Setup(ds => ds.GetDataAsync(cancellationToken))
            .ReturnsAsync(expectedResponse);

        // Act
        await _service.GetOrganizationStructureAsync(cancellationToken);

        // Assert
        _mockDataSource.Verify(
            ds => ds.GetDataAsync(cancellationToken),
            Times.Once);
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