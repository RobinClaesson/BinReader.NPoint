using System.Text.Json;
using Fortitude.Client;
using Microsoft.Extensions.DependencyInjection;
using NPoint.Net.Caching;
using NPoint.Net.Config;
using NSubstitute;

namespace NPoint.Net.Tests;

public class NPointClientTests
{
    private static string GetRandomDocumentId() => Guid.NewGuid().ToString("N")[..20];

    private static INPointClient CreateClient(FortitudeClient fortitude, Action<NPointConfig>? configuration = null)
    {
        var services = new ServiceCollection();
        services.AddHttpClient();
        services.AddFortitudeClient(fortitude);

        if (configuration is null)
            services.AddNPoint();
        else
            services.AddNPoint(configuration);

        return services.BuildServiceProvider().GetRequiredService<INPointClient>();
    }

    private record TestData(string Foo);

    private static (FortitudeClient fortitude, FortitudeHandler handler) SetupNPointCall(string? json)
    {
        var fortitude = FortitudeClient.Create();
        var handler = fortitude.Accepts()
            .Get()
            .HttpRoute("/{id}")
            .Returns((_, res) => res.Ok(json));

        return (fortitude, handler);
    }

    [Fact]
    public async Task GetAsync_WhenCacheIsNull_SendsRequestToNPoint()
    {
        var documentId = GetRandomDocumentId();
        const string documentJson = """
                                    { "Foo": "bar" }
                                    """;

        var (fortitude, handler) = SetupNPointCall(documentJson);
        var target = CreateClient(fortitude, cfg => cfg.NoCache());

        var result = await target.GetAsync(documentId);

        Assert.Equal(documentJson, result);

        var clientRequest = Assert.Single(handler.ReceivedRequests);
        Assert.Equal($"/{documentId}", clientRequest.Route);
    }

    [Fact]
    public async Task GetAsync_WhenCacheIsMiss_SendsRequestToNPoint_AndWritesToCache()
    {
        var documentId = GetRandomDocumentId();
        const string documentJson = """
                                    { "Foo": "bar" }
                                    """;

        var mockCache = Substitute.For<INPointCache>();
        mockCache.ContainsAsync(Arg.Any<string>()).Returns(false);

        var (fortitude, handler) = SetupNPointCall(documentJson);
        var target = CreateClient(fortitude, cfg => cfg.UseCache(mockCache));

        var result = await target.GetAsync(documentId);

        Assert.Equal(documentJson, result);

        var clientRequest = Assert.Single(handler.ReceivedRequests);
        Assert.Equal($"/{documentId}", clientRequest.Route);

        await mockCache.Received(1).ContainsAsync(documentId);
        await mockCache.Received(1).SetAsync(documentId, documentJson);
        await mockCache.DidNotReceive().GetAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task GetAsync_WhenCacheIsHit_ReturnsCachedValue()
    {
        var documentId = GetRandomDocumentId();
        const string documentJson = """
                                    { "Foo": "bar" }
                                    """;

        var mockCache = Substitute.For<INPointCache>();
        mockCache.ContainsAsync(Arg.Any<string>()).Returns(true);
        mockCache.GetAsync(documentId).Returns(documentJson);

        var (fortitude, handler) = SetupNPointCall(documentJson);
        var target = CreateClient(fortitude, cfg => cfg.UseCache(mockCache));

        var result = await target.GetAsync(documentId);

        Assert.Equal(documentJson, result);

        Assert.Empty(handler.ReceivedRequests);

        await mockCache.Received(1).ContainsAsync(documentId);
        await mockCache.Received(1).GetAsync(documentId);
        await mockCache.DidNotReceive().SetAsync(Arg.Any<string>(), Arg.Any<string>());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task GetAsync_WhenDocumentIdIsEmpty_ThrowsException(string? documentId)
    {
        var mockCache = Substitute.For<INPointCache>();

        var (fortitude, handler) = SetupNPointCall(null);
        var target = CreateClient(fortitude, cfg => cfg.UseCache(mockCache));

        await Assert.ThrowsAsync<ArgumentException>(async () => await target.GetAsync(documentId!));

        Assert.Empty(handler.ReceivedRequests);

        await mockCache.DidNotReceive().ContainsAsync(Arg.Any<string>());
        await mockCache.DidNotReceive().GetAsync(Arg.Any<string>());
        await mockCache.DidNotReceive().SetAsync(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task GetAsyncT_WhenCacheIsNull_SendsRequestToNPoint()
    {
        var documentId = GetRandomDocumentId();
        const string documentJson = """
                                    { "Foo": "bar" }
                                    """;
        var expectedData = new TestData("bar");

        var (fortitude, handler) = SetupNPointCall(documentJson);
        var target = CreateClient(fortitude, cfg => cfg.NoCache());

        var result = await target.GetAsync<TestData>(documentId);

        Assert.Equal(expectedData, result);

        var clientRequest = Assert.Single(handler.ReceivedRequests);
        Assert.Equal($"/{documentId}", clientRequest.Route);
    }

    [Fact]
    public async Task GetAsyncT_WhenCacheIsMiss_SendsRequestToNPoint_AndWritesToCache()
    {
        var documentId = GetRandomDocumentId();
        const string documentJson = """
                                    { "Foo": "bar" }
                                    """;
        var expectedData = new TestData("bar");

        var mockCache = Substitute.For<INPointCache>();
        mockCache.ContainsAsync(Arg.Any<string>()).Returns(false);

        var (fortitude, handler) = SetupNPointCall(documentJson);
        var target = CreateClient(fortitude, cfg => cfg.UseCache(mockCache));

        var result = await target.GetAsync<TestData>(documentId);

        Assert.Equal(expectedData, result);

        var clientRequest = Assert.Single(handler.ReceivedRequests);
        Assert.Equal($"/{documentId}", clientRequest.Route);

        await mockCache.Received(1).ContainsAsync(documentId);
        await mockCache.Received(1).SetAsync(documentId, documentJson);
        await mockCache.DidNotReceive().GetAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task GetAsyncT_WhenCacheIsHit_ReturnsCachedValue()
    {
        var documentId = GetRandomDocumentId();
        const string documentJson = """
                                    { "Foo": "bar" }
                                    """;
        var expectedData = new TestData("bar");

        var mockCache = Substitute.For<INPointCache>();
        mockCache.ContainsAsync(Arg.Any<string>()).Returns(true);
        mockCache.GetAsync(documentId).Returns(documentJson);

        var (fortitude, handler) = SetupNPointCall(documentJson);
        var target = CreateClient(fortitude, cfg => cfg.UseCache(mockCache));

        var result = await target.GetAsync<TestData>(documentId);

        Assert.Equal(expectedData, result);

        Assert.Empty(handler.ReceivedRequests);

        await mockCache.Received(1).ContainsAsync(documentId);
        await mockCache.Received(1).GetAsync(documentId);
        await mockCache.DidNotReceive().SetAsync(Arg.Any<string>(), Arg.Any<string>());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task GetAsyncT_WhenDocumentIdIsEmpty_ThrowsException(string? documentId)
    {
        var mockCache = Substitute.For<INPointCache>();

        var (fortitude, handler) = SetupNPointCall(null);
        var target = CreateClient(fortitude, cfg => cfg.UseCache(mockCache));

        await Assert.ThrowsAsync<ArgumentException>(async () => await target.GetAsync<TestData>(documentId!));

        Assert.Empty(handler.ReceivedRequests);

        await mockCache.DidNotReceive().ContainsAsync(Arg.Any<string>());
        await mockCache.DidNotReceive().GetAsync(Arg.Any<string>());
        await mockCache.DidNotReceive().SetAsync(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task GetAsyncT_WhenJsonOptionsIsConfigured_UsesJsonOptions()
    {
        var documentId = GetRandomDocumentId();
        const string documentJson = """
                                    { "foo": "bar" }
                                    """;
        var expectedData = new TestData("bar");

        var (fortitude, handler) = SetupNPointCall(documentJson);
        var target = CreateClient(fortitude, cfg =>
        {
            cfg.NoCache();
            cfg.JsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        });

        var result = await target.GetAsync<TestData>(documentId);

        Assert.Equal(expectedData, result);

        var clientRequest = Assert.Single(handler.ReceivedRequests);
        Assert.Equal($"/{documentId}", clientRequest.Route);
    }
}