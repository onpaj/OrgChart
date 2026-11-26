using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Moq;
using OrgChart.API.Authorization;
using OrgChart.API.Services;
using System.Security.Claims;

namespace OrgChart.API.Tests.Unit.Authorization;

public class OrgChartAuthorizationHandlerTests
{
    private readonly Mock<IUserPermissionService> _permissions = new();
    private readonly OrgChartAuthorizationHandler _handler;

    public OrgChartAuthorizationHandlerTests()
    {
        _handler = new OrgChartAuthorizationHandler(_permissions.Object);
    }

    private static ClaimsPrincipal AuthenticatedUser() =>
        new(new ClaimsIdentity(new[] { new Claim("oid", "user-oid") }, "Test"));

    private static AuthorizationHandlerContext ContextFor(ClaimsPrincipal user, OrgChartAccessLevel level) =>
        new(new[] { new OrgChartRequirement(level) }, user, null);

    [Fact]
    public async Task WhenUserIsNotAuthenticated_DoesNotSucceedAndSkipsHeblo()
    {
        var context = ContextFor(new ClaimsPrincipal(new ClaimsIdentity()), OrgChartAccessLevel.Read);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
        _permissions.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public async Task ReadRequirement_FollowsCanRead(bool canRead, bool expected)
    {
        _permissions.Setup(p => p.CanReadAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(canRead);
        var context = ContextFor(AuthenticatedUser(), OrgChartAccessLevel.Read);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().Be(expected);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public async Task WriteRequirement_FollowsCanEdit(bool canEdit, bool expected)
    {
        _permissions.Setup(p => p.CanEditAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(canEdit);
        var context = ContextFor(AuthenticatedUser(), OrgChartAccessLevel.Write);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().Be(expected);
    }

    [Fact]
    public async Task WriteRequirement_DoesNotSucceedOnReadPermissionAlone()
    {
        _permissions.Setup(p => p.CanReadAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(true);
        _permissions.Setup(p => p.CanEditAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(false);
        var context = ContextFor(AuthenticatedUser(), OrgChartAccessLevel.Write);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }
}
