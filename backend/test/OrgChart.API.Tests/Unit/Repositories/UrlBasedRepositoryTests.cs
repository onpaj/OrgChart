using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using OrgChart.API.Configuration;
using OrgChart.API.DataSources;
using OrgChart.API.Models;
using OrgChart.API.Repositories;

namespace OrgChart.API.Tests.Unit.Repositories;

public class UrlBasedRepositoryTests
{
    private readonly Mock<IOrgChartDataSource> _mockDataSource;
    private readonly Mock<ILogger<UrlBasedRepository>> _mockLogger;
    private readonly Mock<IOptions<OrgChartOptions>> _mockOptions;
    private readonly OrgChartOptions _options;
    private readonly UrlBasedRepository _repository;

    public UrlBasedRepositoryTests()
    {
        _mockDataSource = new Mock<IOrgChartDataSource>();
        _mockLogger = new Mock<ILogger<UrlBasedRepository>>();
        _mockOptions = new Mock<IOptions<OrgChartOptions>>();
        
        _options = new OrgChartOptions
        {
            DataSourceUrl = "https://api.example.com/orgchart",
            Permissions = new RepositoryPermissions
            {
                InsertEnabled = false,
                UpdateEnabled = false,
                DeleteEnabled = false
            }
        };
        
        _mockOptions.Setup(o => o.Value).Returns(_options);
        
        _repository = new UrlBasedRepository(_mockDataSource.Object, _mockOptions.Object, _mockLogger.Object);
    }

    [Fact]
    public void InsertEnabled_ShouldReturnConfiguredValue()
    {
        // Arrange & Act & Assert
        _repository.InsertEnabled.Should().Be(_options.Permissions.InsertEnabled);
    }

    [Fact]
    public void UpdateEnabled_ShouldReturnConfiguredValue()
    {
        // Arrange & Act & Assert
        _repository.UpdateEnabled.Should().Be(_options.Permissions.UpdateEnabled);
    }

    [Fact]
    public void DeleteEnabled_ShouldReturnConfiguredValue()
    {
        // Arrange & Act & Assert
        _repository.DeleteEnabled.Should().Be(_options.Permissions.DeleteEnabled);
    }

    [Fact]
    public async Task GetDataAsync_ShouldDelegateToDataSource()
    {
        // Arrange
        var expectedResponse = CreateSampleOrgChartResponse();
        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;
        
        _mockDataSource
            .Setup(ds => ds.GetDataAsync(cancellationToken))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _repository.GetDataAsync(cancellationToken);

        // Assert
        result.Should().BeEquivalentTo(expectedResponse);
        _mockDataSource.Verify(ds => ds.GetDataAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task CreatePositionAsync_WhenInsertDisabled_ShouldThrowNotSupportedException()
    {
        // Arrange
        var position = new Position { Id = "pos1", Title = "Test Position" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotSupportedException>(
            () => _repository.CreatePositionAsync(position));
        
        exception.Message.Should().Contain("Insert operations are not enabled");
        exception.Message.Should().Contain("OrgChart:Permissions:InsertEnabled");
    }

    [Fact]
    public async Task CreatePositionAsync_WhenInsertEnabled_ShouldThrowNotImplementedException()
    {
        // Arrange
        _options.Permissions.InsertEnabled = true;
        var position = new Position { Id = "pos1", Title = "Test Position" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotImplementedException>(
            () => _repository.CreatePositionAsync(position));
        
        exception.Message.Should().Contain("URL-based repository is read-only");
        exception.Message.Should().Contain("Position creation is not supported");
    }

    [Fact]
    public async Task UpdatePositionAsync_WhenUpdateDisabled_ShouldThrowNotSupportedException()
    {
        // Arrange
        var position = new Position { Id = "pos1", Title = "Updated Position" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotSupportedException>(
            () => _repository.UpdatePositionAsync(position));
        
        exception.Message.Should().Contain("Update operations are not enabled");
        exception.Message.Should().Contain("OrgChart:Permissions:UpdateEnabled");
    }

    [Fact]
    public async Task UpdatePositionAsync_WhenUpdateEnabled_ShouldThrowNotImplementedException()
    {
        // Arrange
        _options.Permissions.UpdateEnabled = true;
        var position = new Position { Id = "pos1", Title = "Updated Position" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotImplementedException>(
            () => _repository.UpdatePositionAsync(position));
        
        exception.Message.Should().Contain("URL-based repository is read-only");
        exception.Message.Should().Contain("Position updates are not supported");
    }

    [Fact]
    public async Task DeletePositionAsync_WhenDeleteDisabled_ShouldThrowNotSupportedException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotSupportedException>(
            () => _repository.DeletePositionAsync("pos1"));
        
        exception.Message.Should().Contain("Delete operations are not enabled");
        exception.Message.Should().Contain("OrgChart:Permissions:DeleteEnabled");
    }

    [Fact]
    public async Task DeletePositionAsync_WhenDeleteEnabled_ShouldThrowNotImplementedException()
    {
        // Arrange
        _options.Permissions.DeleteEnabled = true;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotImplementedException>(
            () => _repository.DeletePositionAsync("pos1"));
        
        exception.Message.Should().Contain("URL-based repository is read-only");
        exception.Message.Should().Contain("Position deletion is not supported");
    }

    [Fact]
    public async Task CreateEmployeeAsync_WhenInsertDisabled_ShouldThrowNotSupportedException()
    {
        // Arrange
        var employee = new Employee { Id = "emp1", Name = "Test Employee" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotSupportedException>(
            () => _repository.CreateEmployeeAsync("pos1", employee));
        
        exception.Message.Should().Contain("Insert operations are not enabled");
        exception.Message.Should().Contain("OrgChart:Permissions:InsertEnabled");
    }

    [Fact]
    public async Task CreateEmployeeAsync_WhenInsertEnabled_ShouldThrowNotImplementedException()
    {
        // Arrange
        _options.Permissions.InsertEnabled = true;
        var employee = new Employee { Id = "emp1", Name = "Test Employee" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotImplementedException>(
            () => _repository.CreateEmployeeAsync("pos1", employee));
        
        exception.Message.Should().Contain("URL-based repository is read-only");
        exception.Message.Should().Contain("Employee creation is not supported");
    }

    [Fact]
    public async Task UpdateEmployeeAsync_WhenUpdateDisabled_ShouldThrowNotSupportedException()
    {
        // Arrange
        var employee = new Employee { Id = "emp1", Name = "Updated Employee" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotSupportedException>(
            () => _repository.UpdateEmployeeAsync("pos1", employee));
        
        exception.Message.Should().Contain("Update operations are not enabled");
        exception.Message.Should().Contain("OrgChart:Permissions:UpdateEnabled");
    }

    [Fact]
    public async Task UpdateEmployeeAsync_WhenUpdateEnabled_ShouldThrowNotImplementedException()
    {
        // Arrange
        _options.Permissions.UpdateEnabled = true;
        var employee = new Employee { Id = "emp1", Name = "Updated Employee" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotImplementedException>(
            () => _repository.UpdateEmployeeAsync("pos1", employee));
        
        exception.Message.Should().Contain("URL-based repository is read-only");
        exception.Message.Should().Contain("Employee updates are not supported");
    }

    [Fact]
    public async Task DeleteEmployeeAsync_WhenDeleteDisabled_ShouldThrowNotSupportedException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotSupportedException>(
            () => _repository.DeleteEmployeeAsync("pos1", "emp1"));
        
        exception.Message.Should().Contain("Delete operations are not enabled");
        exception.Message.Should().Contain("OrgChart:Permissions:DeleteEnabled");
    }

    [Fact]
    public async Task DeleteEmployeeAsync_WhenDeleteEnabled_ShouldThrowNotImplementedException()
    {
        // Arrange
        _options.Permissions.DeleteEnabled = true;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotImplementedException>(
            () => _repository.DeleteEmployeeAsync("pos1", "emp1"));
        
        exception.Message.Should().Contain("URL-based repository is read-only");
        exception.Message.Should().Contain("Employee deletion is not supported");
    }

    [Fact]
    public void PermissionsEnabled_ShouldReflectConfiguration()
    {
        // Arrange
        var enabledOptions = new OrgChartOptions
        {
            Permissions = new RepositoryPermissions
            {
                InsertEnabled = true,
                UpdateEnabled = true,
                DeleteEnabled = true
            }
        };
        
        var mockEnabledOptions = new Mock<IOptions<OrgChartOptions>>();
        mockEnabledOptions.Setup(o => o.Value).Returns(enabledOptions);
        
        var enabledRepository = new UrlBasedRepository(_mockDataSource.Object, mockEnabledOptions.Object, _mockLogger.Object);

        // Act & Assert
        enabledRepository.InsertEnabled.Should().BeTrue();
        enabledRepository.UpdateEnabled.Should().BeTrue();
        enabledRepository.DeleteEnabled.Should().BeTrue();
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
                    }
                }
            }
        };
    }
}