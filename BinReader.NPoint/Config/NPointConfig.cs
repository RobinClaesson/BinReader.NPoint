using System.Text.Json;
using BinReader.NPoint.Caching;
using Microsoft.Extensions.DependencyInjection;

namespace BinReader.NPoint.Config;

/// <summary>
/// Configures document caching, JSON deserialization, and the npoint client's service lifetime.
/// </summary>
/// <remarks>
/// By default, caching uses <see cref="NPointFileSystemCache"/> with a five-minute time to live.
/// </remarks>
public class NPointConfig
{
    internal INPointCache? Cache { get; set; } = new NPointFileSystemCache();

    /// <summary>
    /// Sets the cache used by newly created npoint clients.
    /// </summary>
    /// <param name="cache">The cache instance used to store and retrieve JSON documents.</param>
    public void UseCache(INPointCache cache) => Cache = cache;

    /// <summary>
    /// Disables caching for newly created npoint clients so each retrieval sends an HTTP request.
    /// </summary>
    public void NoCache() => Cache = null;

    /// <summary>
    /// Gets or sets the serializer options used to deserialize documents retrieved by
    /// <see cref="INPointClient.GetAsync{T}(string, CancellationToken?)"/>.
    /// </summary>
    /// <value>
    /// The deserialization options. The default is a new <see cref="System.Text.Json.JsonSerializerOptions"/>
    /// instance with default settings.
    /// </value>
    public JsonSerializerOptions JsonSerializerOptions { get; set; } = new();

    /// <summary>
    /// Gets or sets the dependency injection lifetime used to register <see cref="INPointClient"/>.
    /// </summary>
    /// <value>The client service lifetime. The default is <see cref="Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped"/>.</value>
    public ServiceLifetime ServiceLifetime { get; set; } = ServiceLifetime.Scoped;
}