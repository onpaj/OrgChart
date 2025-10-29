using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OrgChart.API.Exceptions;
using OrgChart.API.Models;
using OrgChart.API.Repositories;
using OrgChart.API.Services;

namespace OrgChart.API.Tests.Unit.Services;

public class OrgChartServiceTests
{
    private readonly Mock<IOrgChartRepository> _mockRepository;
    private readonly Mock<ILogger<OrgChartService>> _mockLogger;
    private readonly OrgChartService _service;

    public OrgChartServiceTests()
    {
        _mockRepository = new Mock<IOrgChartRepository>();
        _mockLogger = new Mock<ILogger<OrgChartService>>();
        
        _service = new OrgChartService(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetOrganizationStructureAsync_WhenRepositorySucceeds_ShouldReturnData()
    {
        // Arrange
        var expectedResponse = CreateSampleOrgChartResponse();
        
        _mockRepository
            .Setup(r => r.GetDataAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);
        
        _mockRepository.Setup(r => r.InsertEnabled).Returns(false);
        _mockRepository.Setup(r => r.UpdateEnabled).Returns(false);
        _mockRepository.Setup(r => r.DeleteEnabled).Returns(false);

        // Act
        var result = await _service.GetOrganizationStructureAsync();

        // Assert
        result.Should().BeEquivalentTo(expectedResponse);
        
        _mockRepository.Verify(
            r => r.GetDataAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetOrganizationStructureAsync_WhenRepositoryFails_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var dataSourceException = new DataSourceException("Repository error", new Exception("Inner error"));

        _mockRepository
            .Setup(r => r.GetDataAsync(It.IsAny<CancellationToken>()))
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
        
        _mockRepository
            .Setup(r => r.GetDataAsync(It.IsAny<CancellationToken>()))
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
        
        _mockRepository
            .Setup(r => r.GetDataAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);
        
        _mockRepository.Setup(r => r.InsertEnabled).Returns(true);
        _mockRepository.Setup(r => r.UpdateEnabled).Returns(false);
        _mockRepository.Setup(r => r.DeleteEnabled).Returns(true);

        // Act
        await _service.GetOrganizationStructureAsync();

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Retrieving organizational structure from repository")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString()!.Contains("Successfully loaded organizational structure") &&
                    v.ToString()!.Contains("Insert=True") &&
                    v.ToString()!.Contains("Update=False") &&
                    v.ToString()!.Contains("Delete=True")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetOrganizationStructureAsync_WhenRepositoryExceptionOccurs_ShouldLogError()
    {
        // Arrange
        var dataSourceException = new DataSourceException("Repository error");
        
        _mockRepository
            .Setup(r => r.GetDataAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(dataSourceException);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetOrganizationStructureAsync());

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Repository error while retrieving organizational structure")),
                dataSourceException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetOrganizationStructureAsync_WithCancellationToken_ShouldPassTokenToRepository()
    {
        // Arrange
        var expectedResponse = CreateSampleOrgChartResponse();
        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;
        
        _mockRepository
            .Setup(r => r.GetDataAsync(cancellationToken))
            .ReturnsAsync(expectedResponse);
        
        _mockRepository.Setup(r => r.InsertEnabled).Returns(false);
        _mockRepository.Setup(r => r.UpdateEnabled).Returns(false);
        _mockRepository.Setup(r => r.DeleteEnabled).Returns(false);

        // Act
        await _service.GetOrganizationStructureAsync(cancellationToken);

        // Assert
        _mockRepository.Verify(
            r => r.GetDataAsync(cancellationToken),
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