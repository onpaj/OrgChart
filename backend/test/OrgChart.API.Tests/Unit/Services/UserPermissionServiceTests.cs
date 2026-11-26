using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using OrgChart.API.Services;
using System.Security.Claims;

namespace OrgChart.API.Tests.Unit.Services;

public class HebloUserPermissionServiceTests
{
    private const string ObjectId = "user-oid";
    private const string Token = "user-token";

    private readonly Mock<IHebloPermissionClient> _client = new();
    private readonly DefaultHttpContext _httpContext = new();
    private readonly HebloUserPermissionService _service;

    public HebloUserPermissionServiceTests()
    {
        _httpContext.Request.Headers.Authorization = $"Bearer {Token}";
        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(a => a.HttpContext).Returns(_httpContext);
        _service = new HebloUserPermissionService(_client.Object, accessor.Object);
    }

    private static ClaimsPrincipal AuthenticatedUser(string? objectId = ObjectId)
    {
        var claims = objectId is null ? Array.Empty<Claim>() : new[] { new Claim("oid", objectId) };
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
    }

    private void GivenHebloReturns(bool isSuperUser, params string[] permissions) =>
        _client.Setup(c => c.GetPermissionsAsync(ObjectId, Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HebloPermissions(isSuperUser, permissions));

    [Fact]
    public async Task CanReadAsync_WithReadPermission_ReturnsTrue()
    {
        GivenHebloReturns(false, HebloPermissionNames.OrgChartRead);

        var result = await _service.CanReadAsync(AuthenticatedUser());

        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanReadAsync_WithoutReadPermission_ReturnsFalse()
    {
        GivenHebloReturns(false, "products.catalog.read");

        var result = await _service.CanReadAsync(AuthenticatedUser());

        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanEditAsync_WithWritePermission_ReturnsTrue()
    {
        GivenHebloReturns(false, HebloPermissionNames.OrgChartRead, HebloPermissionNames.OrgChartWrite);

        var result = await _service.CanEditAsync(AuthenticatedUser());

        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanEditAsync_WithOnlyReadPermission_ReturnsFalse()
    {
        GivenHebloReturns(false, HebloPermissionNames.OrgChartRead);

        var result = await _service.CanEditAsync(AuthenticatedUser());

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SuperUser_CanReadAndEditWithoutExplicitPermissions()
    {
        GivenHebloReturns(true);

        (await _service.CanReadAsync(AuthenticatedUser())).Should().BeTrue();
        (await _service.CanEditAsync(AuthenticatedUser())).Should().BeTrue();
    }

    [Fact]
    public async Task CanEditAsync_WhenUserIsNull_ReturnsFalse()
    {
        var result = await _service.CanEditAsync(null);

        result.Should().BeFalse();
        _client.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CanReadAsync_WhenUserNotAuthenticated_ReturnsFalse()
    {
        var result = await _service.CanReadAsync(new ClaimsPrincipal(new ClaimsIdentity()));

        result.Should().BeFalse();
        _client.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CanReadAsync_WhenObjectIdMissing_ReturnsFalse()
    {
        var result = await _service.CanReadAsync(AuthenticatedUser(objectId: null));

        result.Should().BeFalse();
        _client.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CanReadAsync_ReadsMappedObjectIdClaim()
    {
        GivenHebloReturns(false, HebloPermissionNames.OrgChartRead);
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("http://schemas.microsoft.com/identity/claims/objectidentifier", ObjectId),
        }, "Test"));

        var result = await _service.CanReadAsync(user);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanReadAsync_WhenNoBearerToken_ReturnsFalse()
    {
        _httpContext.Request.Headers.Authorization = string.Empty;

        var result = await _service.CanReadAsync(AuthenticatedUser());

        result.Should().BeFalse();
        _client.VerifyNoOtherCalls();
    }
}

public class MockUserPermissionServiceTests
{
    private readonly MockUserPermissionService _service = new();

    [Fact]
    public async Task MockService_AllowsReadAndEdit()
    {
        (await _service.CanReadAsync(null)).Should().BeTrue();
        (await _service.CanEditAsync(null)).Should().BeTrue();
    }
}
