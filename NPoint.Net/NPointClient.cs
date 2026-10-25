using System.Text.Json;
using NPoint.Net.Caching;
using NPoint.Net.Config;

namespace NPoint.Net;

internal sealed class NPointClient(IHttpClientFactory httpClientFactory, NPointConfig config) : INPointClient
{
    private const string BaseUrl = "https://api.npoint.io/";

    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();
    private readonly INPointCache? _cache = config.Cache;

    public async Task<string> GetAsync(string documentId)
    {
        if (_cache is not null && await _cache.ContainsAsync(documentId))
        {
            return (await _cache.GetAsync(documentId))!;
        }

        var path = $"{BaseUrl}{documentId}";
        var response = await _httpClient.GetAsync(path);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"{response.StatusCode} - {response.ReasonPhrase}");
        }

        var document = await response.Content.ReadAsStringAsync();

        if (_cache is not null)
        {
            await _cache.SetAsync(documentId, document);
        }

        return document;
    }

    public async Task<T?> GetAsync<T>(string documentId)
    {
        var document = await GetAsync(documentId);
        return JsonSerializer.Deserialize<T>(document);
    }
}