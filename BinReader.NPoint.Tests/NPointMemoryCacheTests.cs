using BinReader.NPoint.Caching;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Internal;

namespace BinReader.NPoint.Tests;

public class NPointMemoryCacheTests : IDisposable
{
    private NPointMemoryCache? _target;
    private readonly TestClock _clock = new();
    private MemoryCacheOptions CreateOptions() => new() { Clock = _clock };

    public void Dispose()
    {
        _target?.Dispose();
        _target = null;
        GC.SuppressFinalize(this);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Init_WhenTtlIsZeroOrNegative_ThrowsArgumentOutOfRangeException(int seconds)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _target = new NPointMemoryCache(CreateOptions())
            {
                Ttl = TimeSpan.FromSeconds(seconds)
            }
        );
    }

    [Fact]
    public async Task SetAsyncAndGetAsync_CachesAndReturnsDocument()
    {
        var documentId = TestFixture.GetRandomDocumentId();
        const string documentContent = """
                                       { "Foo": "bar" }
                                       """;

        _target = new NPointMemoryCache(CreateOptions());

        await _target.SetAsync(documentId, documentContent);

        var result = await _target.GetAsync(documentId);
        Assert.Equal(documentContent, result);
    }

    [Fact]
    public async Task ContainsAsync_WhenDocumentIsNotCached_ReturnsFalse()
    {
        var documentId = TestFixture.GetRandomDocumentId();

        _target = new NPointMemoryCache(CreateOptions());

        var result = await _target.ContainsAsync(documentId);
        Assert.False(result);
    }

    [Fact]
    public async Task ContainsAsync_WhenDocumentWasCachedLongerThanTtlAgo_ReturnsFalse()
    {
        var documentId = TestFixture.GetRandomDocumentId();
        const string documentContent = """
                                       { "Foo": "bar" }
                                       """;

        _target = new NPointMemoryCache(CreateOptions())
        {
            Ttl = TimeSpan.FromSeconds(10)
        };

        await _target.SetAsync(documentId, documentContent);
        _clock.UtcNow += TimeSpan.FromSeconds(11);

        var result = await _target.ContainsAsync(documentId);
        Assert.False(result);
    }

    [Fact]
    public async Task ContainsAsync_WhenDocumentWasCachedLessThanTtlAgo_ReturnsTrue()
    {
        var documentId = TestFixture.GetRandomDocumentId();
        const string documentContent = """
                                       { "Foo": "bar" }
                                       """;

        _target = new NPointMemoryCache(CreateOptions())
        {
            Ttl = TimeSpan.FromSeconds(10)
        };

        await _target.SetAsync(documentId, documentContent);
        _clock.UtcNow += TimeSpan.FromSeconds(9);

        var result = await _target.ContainsAsync(documentId);
        Assert.True(result);
    }

    [Fact]
    public async Task GetAsync_WhenDocumentWasCachedLongerThanTtlAgo_ReturnsNull()
    {
        var documentId = TestFixture.GetRandomDocumentId();
        const string documentContent = """
                                       { "Foo": "bar" }
                                       """;

        _target = new NPointMemoryCache(CreateOptions())
        {
            Ttl = TimeSpan.FromSeconds(10)
        };

        await _target.SetAsync(documentId, documentContent);
        _clock.UtcNow += TimeSpan.FromSeconds(11);

        var result = await _target.GetAsync(documentId);
        Assert.Null(result);
    }

    [Fact]
    public async Task ClearAsync_ClearsCache()
    {
        var documentIds = new List<string>
        {
            TestFixture.GetRandomDocumentId(),
            TestFixture.GetRandomDocumentId(),
            TestFixture.GetRandomDocumentId()
        };
        const string documentContent = """
                                       { "Foo": "bar" }
                                       """;

        _target = new NPointMemoryCache(CreateOptions());

        foreach (var documentId in documentIds)
            await _target.SetAsync(documentId, documentContent);

        await _target.ClearAsync();

        foreach (var documentId in documentIds)
        {
            var result = await _target.ContainsAsync(documentId);
            Assert.False(result);
        }
    }

    [Fact]
    public async Task Dispose_PreventsFurtherCacheAccess()
    {
        var documentId = TestFixture.GetRandomDocumentId();
        const string documentContent = """
                                       { "Foo": "bar" }
                                       """;

        var target = new NPointMemoryCache();
        await target.SetAsync(documentId, documentContent);

        target.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await target.GetAsync(documentId));
    }

    private sealed class TestClock : ISystemClock
    {
        public DateTimeOffset UtcNow { get; set; } = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    }
}