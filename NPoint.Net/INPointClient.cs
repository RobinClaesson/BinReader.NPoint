namespace NPoint.Net;

public interface INPointClient
{
    Task<string> GetAsync(string documentId, CancellationToken? cancellationToken = null);
    Task<T?> GetAsync<T>(string documentId, CancellationToken? cancellationToken = null);
}