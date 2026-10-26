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

    /// <summary>
    /// Gets or sets the duration for which a cached file remains valid after its last write.
    /// </summary>
    /// <value>The time to live for cached files. The default is five minutes.</value>
    public TimeSpan Ttl { get; set; } = new TimeSpan(0, 5, 0);

    private static string DocumentPath(string documentId) => Path.Combine(FileSystem.CachePath, $"{documentId}.json");

    /// <summary>
    /// Determines whether a document's cache file exists and has not expired according to <see cref="Ttl"/>.
    /// </summary>
    /// <param name="documentId">The identifier of the cached document.</param>
    /// <returns>
    /// A task whose result is <see langword="true"/> if an unexpired cache file exists;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public Task<bool> ContainsAsync(string documentId)
    {
        var filePath = DocumentPath(documentId);

        if (!FileSystem.FileExists(filePath))
            return Task.FromResult(false);

        var invalidAt = DateTime.UtcNow - Ttl;
        var creationTime = FileSystem.GetLastWriteTimeUtc(filePath);

        return Task.FromResult(creationTime >= invalidAt);
    }

    /// <summary>
    /// Reads an unexpired document from its cache file.
    /// </summary>
    /// <param name="documentId">The identifier of the cached document.</param>
    /// <returns>
    /// A task whose result is the cached JSON content, or <see langword="null"/> if the cache file
    /// is missing or expired.
    /// </returns>
    public async Task<string?> GetAsync(string documentId)
    {
        var contains = await ContainsAsync(documentId);
        return contains ? await FileSystem.ReadAllTextAsync(DocumentPath(documentId)) : null;
    }

    /// <summary>
    /// Writes a document to its cache file, replacing any existing content.
    /// </summary>
    /// <param name="documentId">The identifier used to name the cache file.</param>
    /// <param name="document">The JSON content to cache.</param>
    /// <returns>A task that represents the file write operation.</returns>
    public Task SetAsync(string documentId, string document) =>
        FileSystem.WriteAllTextAsync(DocumentPath(documentId), document);

    /// <summary>
    /// Deletes all files in the shared cache directory, including expired cache files.
    /// </summary>
    /// <returns>A completed task after all files have been deleted.</returns>
    public Task ClearAsync()
    {
        var files = FileSystem.GetDirectoryFiles(FileSystem.CachePath);
        files.ForEach(FileSystem.DeleteFile);
        return Task.CompletedTask;
    }
}