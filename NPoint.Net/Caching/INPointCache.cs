namespace NPoint.Net.Caching;

public interface INPointCache
{
    Task<bool> ContainsAsync(string documentId);
    Task<string?> GetAsync(string documentId);
    Task SetAsync(string documentId, string document);
    Task ClearAsync();
    TimeSpan Ttl { get; set; }
}