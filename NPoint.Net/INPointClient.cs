namespace NPoint.Net;

/// <summary>
/// Retrieves JSON documents from npoint, using a cache when one is configured.
/// </summary>
public interface INPointClient
{
    /// <summary>
    /// Retrieves the JSON content of a document.
    /// </summary>
    /// <param name="documentId">The npoint identifier of the document to retrieve.</param>
    /// <param name="cancellationToken">An optional token used to cancel the HTTP request.</param>
    /// <returns>A task whose result is the document's JSON content.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="documentId"/> is null, empty, or consists only of whitespace.
    /// </exception>
    /// <exception cref="HttpRequestException">
    /// The HTTP request fails or the server returns an unsuccessful status code.
    /// </exception>
    /// <exception cref="OperationCanceledException">The HTTP request is canceled or times out.</exception>
    Task<string> GetAsync(string documentId, CancellationToken? cancellationToken = null);

    /// <summary>
    /// Retrieves a JSON document and deserializes it using the configured serializer options.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the document into.</typeparam>
    /// <param name="documentId">The npoint identifier of the document to retrieve.</param>
    /// <param name="cancellationToken">An optional token used to cancel the HTTP request.</param>
    /// <returns>A task whose result is the deserialized document, which may be <see langword="null"/>.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="documentId"/> is null, empty, or consists only of whitespace.
    /// </exception>
    /// <exception cref="HttpRequestException">
    /// The HTTP request fails or the server returns an unsuccessful status code.
    /// </exception>
    /// <exception cref="OperationCanceledException">The HTTP request is canceled or times out.</exception>
    /// <exception cref="System.Text.Json.JsonException">
    /// The document is not valid JSON or is incompatible with <typeparamref name="T"/>.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// No compatible JSON converter is available for <typeparamref name="T"/> or one of its members.
    /// </exception>
    Task<T?> GetAsync<T>(string documentId, CancellationToken? cancellationToken = null);
}
