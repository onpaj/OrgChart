using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OrgChart.API.Controllers;
using OrgChart.API.Models;
using OrgChart.API.Services;
using System.Security.Claims;

namespace OrgChart.API.Tests.Unit.Controllers;

public class OrgChartControllerTests
{
    private readonly Mock<IOrgChartService> _mockOrgChartService;
    private readonly Mock<ILogger<OrgChartController>> _mockLogger;
    private readonly Mock<IUserPermissionService> _mockPermissionService;
    private readonly OrgChartController _controller;

    public OrgChartControllerTests()
    {
        _mockOrgChartService = new Mock<IOrgChartService>();
        _mockLogger = new Mock<ILogger<OrgChartController>>();
        _mockPermissionService = new Mock<IUserPermissionService>();
        
        _controller = new OrgChartController(
            _mockOrgChartService.Object,
            _mockLogger.Object,
            _mockPermissionService.Object);
    }

    [Fact]
    public async Task GetOrganizationStructure_WhenPermissionServiceAllowsEdit_ShouldSetPermissionsToTrue()
    {
        // Arrange
        var expectedResponse = CreateSampleOrgChartResponse();
        _mockPermissionService.Setup(s => s.CanEdit(It.IsAny<ClaimsPrincipal>()))
            .Returns(true);
        _mockOrgChartService.Setup(s => s.GetOrganizationStructureAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetOrganizationStructure(CancellationToken.None);

        // Assert
        result.Should().BeOfType<ActionResult<OrgChartResponse>>();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<OrgChartResponse>().Subject;
        response.Permissions.CanEdit.Should().BeTrue();
    }

    [Fact]
    public async Task GetOrganizationStructure_WhenPermissionServiceDeniesEdit_ShouldSetPermissionsToFalse()
    {
        // Arrange
        var expectedResponse = CreateSampleOrgChartResponse();
        _mockPermissionService.Setup(s => s.CanEdit(It.IsAny<ClaimsPrincipal>()))
            .Returns(false);
        _mockOrgChartService.Setup(s => s.GetOrganizationStructureAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);
        
        var mockIdentity = new Mock<ClaimsIdentity>();
        mockIdentity.Setup(i => i.IsAuthenticated).Returns(false);
        var mockPrincipal = new Mock<ClaimsPrincipal>();
        mockPrincipal.Setup(p => p.Identity).Returns(mockIdentity.Object);
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = mockPrincipal.Object
            }
        };

        // Act
        var result = await _controller.GetOrganizationStructure(CancellationToken.None);

        // Assert
        result.Should().BeOfType<ActionResult<OrgChartResponse>>();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<OrgChartResponse>().Subject;
        response.Permissions.CanEdit.Should().BeFalse();
    }

    [Fact]
    public async Task GetOrganizationStructure_WhenServiceReturnsData_ShouldReturnOkResult()
    {
        // Arrange
        var expectedResponse = CreateSampleOrgChartResponse();
        _mockPermissionService.Setup(s => s.CanEdit(It.IsAny<ClaimsPrincipal>()))
            .Returns(true);
        _mockOrgChartService.Setup(s => s.GetOrganizationStructureAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var mockIdentity = new Mock<ClaimsIdentity>();
        mockIdentity.Setup(i => i.IsAuthenticated).Returns(true);
        var mockPrincipal = new Mock<ClaimsPrincipal>();
        mockPrincipal.Setup(p => p.Identity).Returns(mockIdentity.Object);
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = mockPrincipal.Object
            }
        };

        // Act
        var result = await _controller.GetOrganizationStructure(CancellationToken.None);

        // Assert
        result.Should().BeOfType<ActionResult<OrgChartResponse>>();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<OrgChartResponse>().Subject;
        response.Organization.Should().BeEquivalentTo(expectedResponse.Organization);
    }

    [Fact]
    public async Task GetOrganizationStructure_WhenServiceThrowsException_ShouldReturnInternalServerError()
    {
        // Arrange
        var exceptionMessage = "Service error";
        _mockPermissionService.Setup(s => s.CanEdit(It.IsAny<ClaimsPrincipal>()))
            .Returns(true);
        _mockOrgChartService.Setup(s => s.GetOrganizationStructureAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(exceptionMessage));

        // Act
        var result = await _controller.GetOrganizationStructure(CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public async Task GetOrganizationStructure_ShouldLogInformation()
    {
        // Arrange
        var expectedResponse = CreateSampleOrgChartResponse();
        _mockPermissionService.Setup(s => s.CanEdit(It.IsAny<ClaimsPrincipal>()))
            .Returns(true);
        _mockOrgChartService.Setup(s => s.GetOrganizationStructureAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        await _controller.GetOrganizationStructure(CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Fetching organizational structure")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetOrganizationStructure_WhenServiceThrowsException_ShouldLogError()
    {
        // Arrange
        var exception = new InvalidOperationException("Service error");
        _mockPermissionService.Setup(s => s.CanEdit(It.IsAny<ClaimsPrincipal>()))
            .Returns(true);
        _mockOrgChartService.Setup(s => s.GetOrganizationStructureAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        await _controller.GetOrganizationStructure(CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error fetching organizational structure")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
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