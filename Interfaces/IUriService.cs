namespace filescrubberMCP.Interfaces;
using filescrubberMCP.Models;

/// <summary>
/// Service interface for sample operations
/// </summary>
public interface IUriService
{
    /// <summary>
    /// Sends a GET request to the specified URI
    /// </summary>
    /// <param name="uri">The URI to send the request to</param>
    /// <param name="headers">Optional headers to include in the request</param>
    /// <returns>The response content as a string</returns>
    Task<string> GetAsync(string uri, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Sends a GET request to the specified URI
    /// </summary>
    /// <param name="uri">The URI to send the request to</param>
    /// <param name="headers">Optional headers to include in the request</param>
    /// <returns>The response content as a string</returns>
    Task<string> GetAsync(string uri, RequestHeaders? headers);

    /// <summary>
    /// Sends a POST request to the specified URI
    /// </summary>
    /// <param name="uri">The URI to send the request to</param>
    /// <param name="jsonBody">Optional JSON body to send with the request</param>
    /// <param name="headers">Optional headers to include in the request</param>
    /// <returns>The response content as a string</returns>
    Task<string> PostAsync(string uri, string? jsonBody = null, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Sends a POST request to the specified URI
    /// </summary>
    /// <param name="uri">The URI to send the request to</param>
    /// <param name="jsonBody">Optional JSON body to send with the request</param>
    /// <param name="headers">Optional headers to include in the request</param>
    /// <returns>The response content as a string</returns>
    Task<string> PostAsync(string uri, string? jsonBody, RequestHeaders? headers);

    /// <summary>
    /// Sends a PUT request to the specified URI
    /// </summary>
    /// <param name="uri">The URI to send the request to</param>
    /// <param name="jsonBody">Optional JSON body to send with the request</param>
    /// <param name="headers">Optional headers to include in the request</param>
    /// <returns>The response content as a string</returns>
    Task<string> PutAsync(string uri, string? jsonBody = null, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Sends a PUT request to the specified URI
    /// </summary>
    /// <param name="uri">The URI to send the request to</param>
    /// <param name="jsonBody">Optional JSON body to send with the request</param>
    /// <param name="headers">Optional headers to include in the request</param>
    /// <returns>The response content as a string</returns>
    Task<string> PutAsync(string uri, string? jsonBody, RequestHeaders? headers);

    /// <summary>
    /// Sends a PATCH request to the specified URI
    /// </summary>
    /// <param name="uri">The URI to send the request to</param>
    /// <param name="jsonBody">Optional JSON body to send with the request</param>
    /// <param name="headers">Optional headers to include in the request</param>
    /// <returns>The response content as a string</returns>
    Task<string> PatchAsync(string uri, string? jsonBody = null, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Sends a PATCH request to the specified URI
    /// </summary>
    /// <param name="uri">The URI to send the request to</param>
    /// <param name="jsonBody">Optional JSON body to send with the request</param>
    /// <param name="headers">Optional headers to include in the request</param>
    /// <returns>The response content as a string</returns>
    Task<string> PatchAsync(string uri, string? jsonBody, RequestHeaders? headers);

    /// <summary>
    /// Sends a DELETE request to the specified URI
    /// </summary>
    /// <param name="uri">The URI to send the request to</param>
    /// <param name="headers">Optional headers to include in the request</param>
    /// <returns>The response content as a string</returns>
    Task<string> DeleteAsync(string uri, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Sends a DELETE request to the specified URI
    /// </summary>
    /// <param name="uri">The URI to send the request to</param>
    /// <param name="headers">Optional headers to include in the request</param>
    /// <returns>The response content as a string</returns>
    Task<string> DeleteAsync(string uri, RequestHeaders? headers);

    /// <summary>
    /// Sends a HEAD request to the specified URI
    /// </summary>
    /// <param name="uri">The URI to send the request to</param>
    /// <param name="headers">Optional headers to include in the request</param>
    /// <returns>The response headers as a dictionary</returns>
    Task<Dictionary<string, string>> HeadAsync(string uri, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Sends a HEAD request to the specified URI
    /// </summary>
    /// <param name="uri">The URI to send the request to</param>
    /// <param name="headers">Optional headers to include in the request</param>
    /// <returns>The response headers as RequestHeaders</returns>
    Task<RequestHeaders> HeadAsync(string uri, RequestHeaders? headers);

    /// <summary>
    /// Sends an OPTIONS request to the specified URI
    /// </summary>
    /// <param name="uri">The URI to send the request to</param>
    /// <param name="headers">Optional headers to include in the request</param>
    /// <returns>The response content as a string</returns>
    Task<string> OptionsAsync(string uri, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Sends an OPTIONS request to the specified URI
    /// </summary>
    /// <param name="uri">The URI to send the request to</param>
    /// <param name="headers">Optional headers to include in the request</param>
    /// <returns>The response content as a string</returns>
    Task<string> OptionsAsync(string uri, RequestHeaders? headers);
}
