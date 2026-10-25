using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using NPoint.Net.Caching;

namespace NPoint.Net.Config;

public class NPointConfig
{
    internal INPointCache? Cache { get; set; } = new NPointFileSystemCache();
    public void UseCache(INPointCache cache) => Cache = cache;
    public void NoCache() => Cache = null;
    public JsonSerializerOptions JsonSerializerOptions { get; set; } = new();
    public ServiceLifetime ServiceLifetime { get; set; } = ServiceLifetime.Scoped;
}