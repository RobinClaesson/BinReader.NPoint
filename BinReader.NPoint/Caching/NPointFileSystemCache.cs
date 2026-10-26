namespace BinReader.NPoint.Caching;

/// <summary>
/// Caches npoint JSON documents as files on the local file system.
/// </summary>
/// <remarks>
/// Documents are stored as <c>{documentId}.json</c> files in the <c>CalcTools</c> directory
/// under <see cref="Environment.SpecialFolder.LocalApplicationData"/>.
/// Cache instances share this directory. Expired files remain on disk until overwritten or cleared.
/// </remarks>
public class NPointFileSystemCache : INPointCache
{
    internal static IFileSystem FileSystem { get; set; } = new FileSystem();

    /// <summary>
    /// Initializes a new cache and creates the cache directory if it does not exist.
    /// </summary>
    public NPointFileSystemCache()
    {
        if (!FileSystem.DirectoryExists(FileSystem.CachePath))
            FileSystem.CreateDirectory(FileSystem.CachePath);
    }

    /// <inheritdoc/>
    public TimeSpan Ttl
    {
        get;
        init
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(value, TimeSpan.Zero);
            field = value;
        }
    } = new(0, 5, 0);

    private static string DocumentPath(string documentId) => Path.Combine(FileSystem.CachePath, $"{documentId}.json");

    /// <inheritdoc/>
    public Task<bool> ContainsAsync(string documentId)
    {
        var filePath = DocumentPath(documentId);

        if (!FileSystem.FileExists(filePath))
            return Task.FromResult(false);

        var invalidAt = DateTime.UtcNow - Ttl;
        var creationTime = FileSystem.GetLastWriteTimeUtc(filePath);

        return Task.FromResult(creationTime >= invalidAt);
    }

    /// <inheritdoc/>
    public async Task<string?> GetAsync(string documentId) => await ContainsAsync(documentId)
        ? await FileSystem.ReadAllTextAsync(DocumentPath(documentId))
        : null;

    /// <inheritdoc/>
    public Task SetAsync(string documentId, string document) =>
        FileSystem.WriteAllTextAsync(DocumentPath(documentId), document);

    /// <inheritdoc/>
    public Task ClearAsync()
    {
        var files = FileSystem.GetDirectoryFiles(FileSystem.CachePath);
        files.ForEach(FileSystem.DeleteFile);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}