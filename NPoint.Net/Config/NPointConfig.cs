using NPoint.Net.Caching;

namespace NPoint.Net.Config;

public class NPointConfig
{
    internal INPointCache? Cache { get; set; } = new NPointFileSystemCache();
    public void UseCache(INPointCache cache) => Cache = cache;
    public void NoCache() => Cache = null;
}