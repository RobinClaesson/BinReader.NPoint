namespace BinReader.NPoint.Caching;

/// <summary>
/// Stores and retrieves npoint JSON documents with a configurable time to live.
/// </summary>
public interface INPointCache
{
    /// <summary>
    /// Determines whether the cache contains an unexpired document.
    /// </summary>
    /// <param name="documentId">The identifier of the cached document.</param>
    /// <returns>
    /// A task whose result is <see langword="true"/> if an unexpired document exists;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    Task<bool> ContainsAsync(string documentId);

    /// <summary>
    /// Retrieves an unexpired document from the cache.
    /// </summary>
    /// <param name="documentId">The identifier of the cached document.</param>
    /// <returns>
    /// A task whose result is the cached JSON content, or <see langword="null"/> if the document
    /// is missing or expired.
    /// </returns>
    Task<string?> GetAsync(string documentId);

    /// <summary>
    /// Stores a document in the cache, replacing any existing content for its identifier.
    /// </summary>
    /// <param name="documentId">The identifier under which to cache the document.</param>
    /// <param name="document">The JSON content to cache.</param>
    /// <returns>A task that represents the cache write operation.</returns>
    Task SetAsync(string documentId, string document);

    /// <summary>
    /// Removes all documents from the cache.
    /// </summary>
    /// <returns>A task that represents the cache clearing operation.</returns>
    Task ClearAsync();

    /// <summary>
    /// Gets or sets the duration for which documents remain valid after they are written.
    /// </summary>
    /// <value>A positive time to live. The default is five minutes.</value>
    /// <remarks>
    /// Reads do not extend expiration.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The value is zero or negative.</exception>
    TimeSpan Ttl { get; init; }
}