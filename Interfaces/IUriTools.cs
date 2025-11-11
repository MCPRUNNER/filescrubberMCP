namespace filescrubberMCP.Interfaces;

/// <summary>
/// Interface for URI/HTTP MCP tools functionality
/// </summary>
public interface IUriTools
{
    /// <summary>
    /// Sends a GET request to the specified URI
    /// </summary>
    /// <param name="uri">The URI to send the request to</param>
    /// <param name="headersJson">Optional headers as JSON string</param>
    /// <returns>JSON representation of the result</returns>
    Task<string> Get(string uri, string? headersJson = null);

    /// <summary>
    /// Sends a POST request to the specified URI
    /// </summary>
    /// <param name="uri">The URI to send the request to</param>
    /// <param name="jsonBody">Optional JSON body</param>
    /// <param name="headersJson">Optional headers as JSON string</param>
    /// <returns>JSON representation of the result</returns>
    Task<string> Post(string uri, string? jsonBody = null, string? headersJson = null);

    /// <summary>
    /// Sends a PUT request to the specified URI
    /// </summary>
    /// <param name="uri">The URI to send the request to</param>
    /// <param name="jsonBody">Optional JSON body</param>
    /// <param name="headersJson">Optional headers as JSON string</param>
    /// <returns>JSON representation of the result</returns>
    Task<string> Put(string uri, string? jsonBody = null, string? headersJson = null);

    /// <summary>
    /// Sends a PATCH request to the specified URI
    /// </summary>
    /// <param name="uri">The URI to send the request to</param>
    /// <param name="jsonBody">Optional JSON body</param>
    /// <param name="headersJson">Optional headers as JSON string</param>
    /// <returns>JSON representation of the result</returns>
    Task<string> Patch(string uri, string? jsonBody = null, string? headersJson = null);

    /// <summary>
    /// Sends a DELETE request to the specified URI
    /// </summary>
    /// <param name="uri">The URI to send the request to</param>
    /// <param name="headersJson">Optional headers as JSON string</param>
    /// <returns>JSON representation of the result</returns>
    Task<string> Delete(string uri, string? headersJson = null);

    /// <summary>
    /// Sends a HEAD request to the specified URI
    /// </summary>
    /// <param name="uri">The URI to send the request to</param>
    /// <param name="headersJson">Optional headers as JSON string</param>
    /// <returns>JSON representation of the result with response headers</returns>
    Task<string> Head(string uri, string? headersJson = null);

    /// <summary>
    /// Sends an OPTIONS request to the specified URI
    /// </summary>
    /// <param name="uri">The URI to send the request to</param>
    /// <param name="headersJson">Optional headers as JSON string</param>
    /// <returns>JSON representation of the result</returns>
    Task<string> Options(string uri, string? headersJson = null);
}
