using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using OrgChart.API.Configuration;
using OrgChart.API.Services;

namespace OrgChart.API.Tests.Unit.Services;

public class HebloPermissionClientTests
{
    private const string UserObjectId = "user-oid";
    private const string UserToken = "incoming-user-token";
    private const string HebloToken = "heblo-obo-token";

    private readonly Mock<IHebloTokenProvider> _tokenProvider = new();
    private readonly Mock<HttpMessageHandler> _httpHandler = new();
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());
    private readonly List<HttpRequestMessage> _requests = new();

    private HebloPermissionClient CreateClient()
    {
        var httpClient = new HttpClient(_httpHandler.Object);
        var options = Options.Create(new HebloOptions
        {
            BaseUrl = "https://heblo.example.com/",
            Scope = "api://heblo/access_as_user",
            CacheMinutes = 5,
        });
        return new HebloPermissionClient(
            httpClient, _tokenProvider.Object, _cache, options, NullLogger<HebloPermissionClient>.Instance);
    }

    private void SetupToken() =>
        _tokenProvider.Setup(p => p.AcquireTokenOnBehalfOfAsync(UserToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(HebloToken);

    private void SetupResponse(HttpStatusCode status, string body) =>
        _httpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, _) => _requests.Add(request))
            .ReturnsAsync(() => new HttpResponseMessage(status) { Content = new StringContent(body) });

    [Fact]
    public async Task GetPermissionsAsync_ReturnsPermissionsFromHebloMe()
    {
        // Arrange
        SetupToken();
        SetupResponse(HttpStatusCode.OK,
            """{"success":true,"isSuperUser":false,"permissions":["anela.org_chart.read","anela.org_chart.write"]}""");
        var client = CreateClient();

        // Act
        var result = await client.GetPermissionsAsync(UserObjectId, UserToken, CancellationToken.None);

        // Assert
        result.IsSuperUser.Should().BeFalse();
        result.Permissions.Should().BeEquivalentTo("anela.org_chart.read", "anela.org_chart.write");
    }

    [Fact]
    public async Task GetPermissionsAsync_CallsAuthMeWithOnBehalfOfToken()
    {
        // Arrange
        SetupToken();
        SetupResponse(HttpStatusCode.OK, """{"permissions":[]}""");
        var client = CreateClient();

        // Act
        await client.GetPermissionsAsync(UserObjectId, UserToken, CancellationToken.None);

        // Assert
        _requests.Should().ContainSingle();
        _requests[0].RequestUri.Should().Be(new Uri("https://heblo.example.com/api/auth/me"));
        _requests[0].Headers.Authorization!.Scheme.Should().Be("Bearer");
        _requests[0].Headers.Authorization!.Parameter.Should().Be(HebloToken);
    }

    [Fact]
    public async Task GetPermissionsAsync_ReadsSuperUserFlag()
    {
        // Arrange
        SetupToken();
        SetupResponse(HttpStatusCode.OK, """{"isSuperUser":true,"permissions":[]}""");
        var client = CreateClient();

        // Act
        var result = await client.GetPermissionsAsync(UserObjectId, UserToken, CancellationToken.None);

        // Assert
        result.IsSuperUser.Should().BeTrue();
    }

    [Fact]
    public async Task GetPermissionsAsync_CachesResultPerUser()
    {
        // Arrange
        SetupToken();
        SetupResponse(HttpStatusCode.OK, """{"permissions":["anela.org_chart.read"]}""");
        var client = CreateClient();

        // Act
        await client.GetPermissionsAsync(UserObjectId, UserToken, CancellationToken.None);
        var second = await client.GetPermissionsAsync(UserObjectId, UserToken, CancellationToken.None);

        // Assert
        _requests.Should().ContainSingle();
        second.Permissions.Should().Contain("anela.org_chart.read");
    }

    [Fact]
    public async Task GetPermissionsAsync_WhenHebloFails_ReturnsNoPermissionsAndDoesNotCache()
    {
        // Arrange
        SetupToken();
        SetupResponse(HttpStatusCode.InternalServerError, "boom");
        var client = CreateClient();

        // Act
        var first = await client.GetPermissionsAsync(UserObjectId, UserToken, CancellationToken.None);
        await client.GetPermissionsAsync(UserObjectId, UserToken, CancellationToken.None);

        // Assert
        first.IsSuperUser.Should().BeFalse();
        first.Permissions.Should().BeEmpty();
        _requests.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPermissionsAsync_WhenTokenExchangeFails_ReturnsNoPermissions()
    {
        // Arrange
        _tokenProvider.Setup(p => p.AcquireTokenOnBehalfOfAsync(UserToken, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("AADSTS65001"));
        SetupResponse(HttpStatusCode.OK, """{"permissions":["anela.org_chart.read"]}""");
        var client = CreateClient();

        // Act
        var result = await client.GetPermissionsAsync(UserObjectId, UserToken, CancellationToken.None);

        // Assert
        result.Permissions.Should().BeEmpty();
        _requests.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPermissionsAsync_WhenCallerCancels_Throws()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        _tokenProvider.Setup(p => p.AcquireTokenOnBehalfOfAsync(UserToken, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException());
        var client = CreateClient();

        // Act
        var act = () => client.GetPermissionsAsync(UserObjectId, UserToken, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
