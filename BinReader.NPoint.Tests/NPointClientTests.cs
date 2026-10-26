using System.Text.Json;
using BinReader.NPoint.Caching;
using BinReader.NPoint.Config;
using NSubstitute;

namespace BinReader.NPoint.Tests;

[Collection(nameof(TestFixture))]
public class NPointClientTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    private record TestData(string Foo);

    [Fact]
    public async Task GetAsync_WhenCacheIsNull_SendsRequestToNPoint()
    {
        var documentId = TestFixture.GetRandomDocumentId();
        const string documentJson = """
                                    { "Foo": "bar" }
                                    """;

        var npointHandler = fixture.SetupNPointHttpCall(documentJson);
        var target = fixture.CreateNPointClient(cfg => cfg.NoCache());

        var result = await target.GetAsync(documentId);
        Assert.Equal(documentJson, result);

        var clientRequest = Assert.Single(npointHandler.ReceivedRequests);
        Assert.Equal($"/{documentId}", clientRequest.Route);
    }

    [Fact]
    public async Task GetAsync_WhenCacheIsMiss_SendsRequestToNPoint_AndWritesToCache()
    {
        var documentId = TestFixture.GetRandomDocumentId();
        const string documentJson = """
                                    { "Foo": "bar" }
                                    """;

        var mockCache = Substitute.For<INPointCache>();
        mockCache.GetAsync(Arg.Any<string>()).Returns((string?)null);

        var npointHandler = fixture.SetupNPointHttpCall(documentJson);
        var target = fixture.CreateNPointClient(cfg => cfg.UseCache(mockCache));

        var result = await target.GetAsync(documentId);
        Assert.Equal(documentJson, result);

        var clientRequest = Assert.Single(npointHandler.ReceivedRequests);
        Assert.Equal($"/{documentId}", clientRequest.Route);

        await mockCache.Received(1).GetAsync(documentId);
        await mockCache.Received(1).SetAsync(documentId, documentJson);
    }

    [Fact]
    public async Task GetAsync_WhenCacheIsHit_ReturnsCachedValue()
    {
        var documentId = TestFixture.GetRandomDocumentId();
        const string documentJson = """
                                    { "Foo": "bar" }
                                    """;

        var mockCache = Substitute.For<INPointCache>();
        mockCache.GetAsync(documentId).Returns(documentJson);

        var npointHandler = fixture.SetupNPointHttpCall(documentJson);
        var target = fixture.CreateNPointClient(cfg => cfg.UseCache(mockCache));

        var result = await target.GetAsync(documentId);
        Assert.Equal(documentJson, result);

        Assert.Empty(npointHandler.ReceivedRequests);

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

        var npointHandler = fixture.SetupNPointHttpCall(null);
        var target = fixture.CreateNPointClient(cfg => cfg.UseCache(mockCache));

        await Assert.ThrowsAsync<ArgumentException>(async () => await target.GetAsync(documentId!));

        Assert.Empty(npointHandler.ReceivedRequests);

        await mockCache.DidNotReceive().ContainsAsync(Arg.Any<string>());
        await mockCache.DidNotReceive().GetAsync(Arg.Any<string>());
        await mockCache.DidNotReceive().SetAsync(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task GetAsyncT_WhenCacheIsNull_SendsRequestToNPoint()
    {
        var documentId = TestFixture.GetRandomDocumentId();
        const string documentJson = """
                                    { "Foo": "bar" }
                                    """;
        var expectedData = new TestData("bar");

        var npointHandler = fixture.SetupNPointHttpCall(documentJson);
        var target = fixture.CreateNPointClient(cfg => cfg.NoCache());

        var result = await target.GetAsync<TestData>(documentId);
        Assert.Equal(expectedData, result);

        var clientRequest = Assert.Single(npointHandler.ReceivedRequests);
        Assert.Equal($"/{documentId}", clientRequest.Route);
    }

    [Fact]
    public async Task GetAsyncT_WhenCacheIsMiss_SendsRequestToNPoint_AndWritesToCache()
    {
        var documentId = TestFixture.GetRandomDocumentId();
        const string documentJson = """
                                    { "Foo": "bar" }
                                    """;
        var expectedData = new TestData("bar");

        var mockCache = Substitute.For<INPointCache>();
        mockCache.GetAsync(Arg.Any<string>()).Returns((string?)null);

        var npointHandler = fixture.SetupNPointHttpCall(documentJson);
        var target = fixture.CreateNPointClient(cfg => cfg.UseCache(mockCache));

        var result = await target.GetAsync<TestData>(documentId);
        Assert.Equal(expectedData, result);

        var clientRequest = Assert.Single(npointHandler.ReceivedRequests);
        Assert.Equal($"/{documentId}", clientRequest.Route);

        await mockCache.Received(1).GetAsync(documentId);
        await mockCache.Received(1).SetAsync(documentId, documentJson);
    }

    [Fact]
    public async Task GetAsyncT_WhenCacheIsHit_ReturnsCachedValue()
    {
        var documentId = TestFixture.GetRandomDocumentId();
        const string documentJson = """
                                    { "Foo": "bar" }
                                    """;
        var expectedData = new TestData("bar");

        var mockCache = Substitute.For<INPointCache>();
        mockCache.GetAsync(documentId).Returns(documentJson);

        var npointHandler = fixture.SetupNPointHttpCall(documentJson);
        var target = fixture.CreateNPointClient(cfg => cfg.UseCache(mockCache));

        var result = await target.GetAsync<TestData>(documentId);
        Assert.Equal(expectedData, result);

        Assert.Empty(npointHandler.ReceivedRequests);

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

        var npointHandler = fixture.SetupNPointHttpCall(null);
        var target = fixture.CreateNPointClient(cfg => cfg.UseCache(mockCache));

        await Assert.ThrowsAsync<ArgumentException>(async () => await target.GetAsync<TestData>(documentId!));
        Assert.Empty(npointHandler.ReceivedRequests);

        await mockCache.DidNotReceive().ContainsAsync(Arg.Any<string>());
        await mockCache.DidNotReceive().GetAsync(Arg.Any<string>());
        await mockCache.DidNotReceive().SetAsync(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task GetAsyncT_WhenJsonOptionsIsConfigured_UsesJsonOptions()
    {
        var documentId = TestFixture.GetRandomDocumentId();
        const string documentJson = """
                                    { "foo": "bar" }
                                    """;
        var expectedData = new TestData("bar");

        var handler = fixture.SetupNPointHttpCall(documentJson);
        var target = fixture.CreateNPointClient(cfg =>
        {
            cfg.NoCache();
            cfg.JsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        });

        var result = await target.GetAsync<TestData>(documentId);
        Assert.Equal(expectedData, result);

        var clientRequest = Assert.Single(handler.ReceivedRequests);
        Assert.Equal($"/{documentId}", clientRequest.Route);
    }

    [Fact]
    public async Task ClearCacheAsync_WhenCacheIsNull_DoesNothing()
    {
        var target = fixture.CreateNPointClient(cfg => cfg.NoCache());

        // To verify no exceptions
        await target.ClearCacheAsync();
    }

    [Fact]
    public async Task ClearCacheAsync_WhenCacheIsConfigured_ClearsCache()
    {
        var mockCache = Substitute.For<INPointCache>();
        var target = fixture.CreateNPointClient(cfg => cfg.UseCache(mockCache));

        await target.ClearCacheAsync();

        await mockCache.Received(1).ClearAsync();
    }

    [Fact]
    public void Dispose_DisposesResources()
    {
        var mockCache = Substitute.For<INPointCache>();
        var mockHttpClient = Substitute.For<HttpClient>();
        var mockHttpClientFactory = Substitute.For<IHttpClientFactory>();
        mockHttpClientFactory.CreateClient().Returns(mockHttpClient);

        var config = new NPointConfig();
        config.UseCache(mockCache);
        var target = new NPointClient(mockHttpClientFactory, config);

        target.Dispose();

        mockCache.Received(1).Dispose();
        mockHttpClient.Received(1).Dispose();
    }
}