namespace NPoint.Net.Caching;

public class NPointFileSystemCache : INPointCache
{
    internal static IFileSystem FileSystem { get; set; } = new FileSystem();

    public NPointFileSystemCache()
    {
        if (!FileSystem.DirectoryExists(FileSystem.CachePath))
            FileSystem.CreateDirectory(FileSystem.CachePath);
    }

    public TimeSpan Ttl { get; set; } = new TimeSpan(0, 5, 0);

    private static string DocumentPath(string documentId) => Path.Combine(FileSystem.CachePath, $"{documentId}.json");

    public Task<bool> ContainsAsync(string documentId)
    {
        var filePath = DocumentPath(documentId);

        if (!FileSystem.FileExists(filePath))
            return Task.FromResult(false);

        var invalidAt = DateTime.UtcNow - Ttl;
        var creationTime = FileSystem.GetLastWriteTimeUtc(filePath);

        return Task.FromResult(creationTime >= invalidAt);
    }

    public async Task<string?> GetAsync(string documentId)
    {
        var contains = await ContainsAsync(documentId);
        return contains ? await FileSystem.ReadAllTextAsync(DocumentPath(documentId)) : null;
    }

    public Task SetAsync(string documentId, string document) =>
        FileSystem.WriteAllTextAsync(DocumentPath(documentId), document);

    public Task ClearAsync()
    {
        var files = FileSystem.GetDirectoryFiles(FileSystem.CachePath);
        files.ForEach(FileSystem.DeleteFile);
        return Task.CompletedTask;
    }
}