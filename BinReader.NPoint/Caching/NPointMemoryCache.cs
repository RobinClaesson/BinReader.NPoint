using Microsoft.Extensions.Caching.Memory;

namespace BinReader.NPoint.Caching;

/// <summary>
/// Caches npoint JSON documents in memory with absolute expiration.
/// </summary>
/// <remarks>
/// Each instance owns its cache. Documents are not shared with other instances or persisted across
/// process restarts. Dispose the cache when the clients using it are no longer needed.
/// </remarks>
public sealed class NPointMemoryCache : INPointCache
{
    private readonly MemoryCache _cache;

    /// <summary>
    /// Initializes a new memory cache
    /// </summary>
    public NPointMemoryCache() : this(new MemoryCacheOptions())
    {
    }

    internal NPointMemoryCache(MemoryCacheOptions options) => _cache = new MemoryCache(options);

    /// <inheritdoc/>
    public TimeSpan Ttl
    {
        get;
        init
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(value, TimeSpan.Zero);
            field = value;
        }
    } = new(0, 5, 0);

    /// <inheritdoc/>
    public Task<bool> ContainsAsync(string documentId) => Task.FromResult(_cache.TryGetValue(documentId, out _));

    /// <inheritdoc/>
    public Task<string?> GetAsync(string documentId) => Task.FromResult(_cache.Get<string>(documentId));

    /// <inheritdoc/>
    public Task SetAsync(string documentId, string document)
    {
        _cache.Set(documentId, document, Ttl);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task ClearAsync()
    {
        _cache.Clear();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void Dispose() => _cache.Dispose();
}