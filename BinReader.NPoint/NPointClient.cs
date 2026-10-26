using System.Text.Json;
using BinReader.NPoint.Caching;
using BinReader.NPoint.Config;

namespace BinReader.NPoint;

internal sealed class NPointClient(IHttpClientFactory httpClientFactory, NPointConfig config) : INPointClient
{
    private const string BaseUrl = "https://api.npoint.io/";

    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();
    private readonly INPointCache? _cache = config.Cache;

    /// <inheritdoc/>
    public async Task<string> GetAsync(string documentId, CancellationToken? cancellationToken = null)
    {
        if (string.IsNullOrWhiteSpace(documentId))
        {
            throw new ArgumentException($"{nameof(documentId)} can not be null or empty", nameof(documentId));
        }

        if (_cache is not null && await _cache.GetAsync(documentId) is { } cachedDocument)
        {
            return cachedDocument;
        }

        var path = $"{BaseUrl}{documentId}";
        var response = await _httpClient.GetAsync(path, cancellationToken ?? CancellationToken.None);

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

    /// <inheritdoc/>
    public async Task<T?> GetAsync<T>(string documentId, CancellationToken? cancellationToken = null)
    {
        var document = await GetAsync(documentId, cancellationToken);
        return JsonSerializer.Deserialize<T>(document, options: config.JsonSerializerOptions);
    }

    /// <inheritdoc/>
    public async Task ClearCacheAsync()
    {
        if (_cache is not null)
        {
            await _cache.ClearAsync();
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _cache?.Dispose();
        _httpClient.Dispose();
    }
}