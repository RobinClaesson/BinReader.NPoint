namespace NPoint.Net.Caching;

public class NPointFileSystemCache : INPointCache
{
    public NPointFileSystemCache()
    {
        if (!Directory.Exists(CachePath))
            Directory.CreateDirectory(CachePath);
    }

    public TimeSpan Ttl { get; set; } = new TimeSpan(0, 5, 0);

    private static readonly string CachePath = Path.Join(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "CalcTools"
    );

    private static string DocumentPath(string documentId) =>
        Path.Combine(CachePath, $"{documentId}.json");

    public Task<bool> ContainsAsync(string documentId)
    {
        var filePath = DocumentPath(documentId);

        if (!File.Exists(filePath))
            return Task.FromResult(false);

        var invalidAt = DateTime.UtcNow - Ttl;
        var creationTime = File.GetLastWriteTimeUtc(filePath);

        return Task.FromResult(creationTime >= invalidAt);
    }

    public async Task<string?> GetAsync(string documentId)
    {
        var contains = await ContainsAsync(documentId);
        return contains ? await File.ReadAllTextAsync(DocumentPath(documentId)) : null;
    }

    public Task SetAsync(string documentId, string document) =>
        File.WriteAllTextAsync(DocumentPath(documentId), document);

    public Task ClearAsync()
    {
        var files = Directory.GetFiles(CachePath).ToList();
        files.ForEach(File.Delete);
        return Task.CompletedTask;
    }
}