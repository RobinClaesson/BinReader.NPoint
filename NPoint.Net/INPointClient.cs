namespace NPoint.Net;

public interface INPointClient
{
    Task<string> GetAsync(string documentId);
    Task<T?> GetAsync<T>(string documentId);
}