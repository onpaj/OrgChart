using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using OrgChart.API.Authorization;
using System.Security.Claims;

namespace OrgChart.API.Tests.Unit.Authorization;

public class OrgChartAuthorizationHandlerTests
{
    private readonly OrgChartAuthorizationHandler _handler;

    public OrgChartAuthorizationHandlerTests()
    {
        _handler = new OrgChartAuthorizationHandler();
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenUserIsNotAuthenticated_ShouldNotSucceed()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        var context = new AuthorizationHandlerContext(
            new[] { new OrgChartRequirement(OrgChartAccessLevel.Read) },
            user,
            null);
        var requirement = new OrgChartRequirement(OrgChartAccessLevel.Read);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenUserAuthenticatedWithReadAccess_ShouldSucceed()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-id"),
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        var requirement = new OrgChartRequirement(OrgChartAccessLevel.Read);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }


    [Fact]
    public async Task HandleRequirementAsync_WhenUserAuthenticatedWithWriteAccessAndHasAdminRole_ShouldSucceed()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-id"),
            new Claim(ClaimTypes.Role, OrgChartClaims.Roles.Admin)
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        var requirement = new OrgChartRequirement(OrgChartAccessLevel.Write);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenUserAuthenticatedWithWriteAccessButNoAdminRole_ShouldNotSucceed()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-id"),
            new Claim(ClaimTypes.Role, "User")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        var requirement = new OrgChartRequirement(OrgChartAccessLevel.Write);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }
}

public class MockOrgChartAuthorizationHandlerTests
{
    private readonly MockOrgChartAuthorizationHandler _handler;

    public MockOrgChartAuthorizationHandlerTests()
    {
        _handler = new MockOrgChartAuthorizationHandler();
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenUserIsNull_ShouldSucceed()
    {
        // Arrange
        var user = new ClaimsPrincipal();
        var requirement = new OrgChartRequirement(OrgChartAccessLevel.Read);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenUserIsNotAuthenticated_ShouldSucceed()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        var requirement = new OrgChartRequirement(OrgChartAccessLevel.Read);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithReadAccess_ShouldSucceed()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        var requirement = new OrgChartRequirement(OrgChartAccessLevel.Read);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithWriteAccess_ShouldSucceed()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        var requirement = new OrgChartRequirement(OrgChartAccessLevel.Write);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }
}