using FluentAssertions;
using OrgChart.API.Authorization;
using OrgChart.API.Services;
using System.Security.Claims;

namespace OrgChart.API.Tests.Unit.Services;

public class RoleBaseUserPermissionServiceTests
{
    private readonly RoleBaseUserPermissionService _service;

    public RoleBaseUserPermissionServiceTests()
    {
        _service = new RoleBaseUserPermissionService();
    }

    [Fact]
    public void CanEdit_WhenUserIsNull_ShouldReturnFalse()
    {
        // Act
        var result = _service.CanEdit(null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanEdit_WhenUserIsNotAuthenticated_ShouldReturnFalse()
    {
        // Arrange
        var identity = new ClaimsIdentity();
        var user = new ClaimsPrincipal(identity);

        // Act
        var result = _service.CanEdit(user);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanEdit_WhenUserIsAuthenticatedButDoesNotHaveAdminRole_ShouldReturnFalse()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-id"),
            new Claim(ClaimTypes.Role, "User")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var user = new ClaimsPrincipal(identity);

        // Act
        var result = _service.CanEdit(user);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanEdit_WhenUserIsAuthenticatedAndHasAdminRole_ShouldReturnTrue()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-id"),
            new Claim(ClaimTypes.Role, OrgChartClaims.Roles.Admin)
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var user = new ClaimsPrincipal(identity);

        // Act
        var result = _service.CanEdit(user);

        // Assert
        result.Should().BeTrue();
    }
}

public class MockUserPermissionServiceTests
{
    private readonly MockUserPermissionService _service;

    public MockUserPermissionServiceTests()
    {
        _service = new MockUserPermissionService();
    }

    [Fact]
    public void CanEdit_WhenUserIsNull_ShouldReturnTrue()
    {
        // Act
        var result = _service.CanEdit(null);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanEdit_WhenUserIsNotAuthenticated_ShouldReturnTrue()
    {
        // Arrange
        var identity = new ClaimsIdentity();
        var user = new ClaimsPrincipal(identity);

        // Act
        var result = _service.CanEdit(user);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanEdit_WhenUserIsAuthenticated_ShouldReturnTrue()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-id")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var user = new ClaimsPrincipal(identity);

        // Act
        var result = _service.CanEdit(user);

        // Assert
        result.Should().BeTrue();
    }
}