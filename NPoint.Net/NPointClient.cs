using System.Text.Json;
using NPoint.Net.Caching;

namespace NPoint.Net;

public class NPointClient(INPointCache cache) : INPointClient
{
    private const string BaseUrl = "https://api.npoint.io/";


    private readonly HttpClient _httpClient = new();

    public async Task<string> GetAsync(string documentId)
    {
        if (await cache.ContainsAsync(documentId))
        {
            return await cache.GetAsync(documentId);
        }

        var path = $"{BaseUrl}{documentId}";
        var response = await _httpClient.GetAsync(path);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"{response.StatusCode} - {response.ReasonPhrase}");
        }

        var document = await response.Content.ReadAsStringAsync();
        await cache.SetAsync(documentId, document);

        return document;
    }

    public async Task<T?> GetAsync<T>(string documentId)
    {
        var document = await GetAsync(documentId);
        return JsonSerializer.Deserialize<T>(document);
    }
}