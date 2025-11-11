using Microsoft.Extensions.Logging;
using filescrubberMCP.Interfaces;
using filescrubberMCP.Models;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace filescrubberMCP.Services;

/// <summary>
/// Service implementation for URI operations
/// </summary>
public class UriService : IUriService
{
    private readonly ILogger<UriService> _logger;
    private readonly IAppConfigurationProvider _configurationProvider;
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the UriService class
    /// </summary>
    /// <param name="logger">Logger for the service</param>
    /// <param name="configurationProvider">Provider for configuration</param>
    /// <param name="httpClient">HTTP client for making requests</param>
    public UriService(
ILogger<UriService> logger,
        IAppConfigurationProvider configurationProvider,
     HttpClient httpClient)
    {
   _logger = logger;
        _configurationProvider = configurationProvider;
        _httpClient = httpClient;
    }

    /// <summary>
    /// Sends a GET request to the specified URI
    /// </summary>
    /// <param name="uri">The URI to send the request to</param>
    /// <param name="headers">Optional headers to include in the request</param>
    /// <returns>The response content as a string</returns>
    public async Task<string> GetAsync(string uri, Dictionary<string, string>? headers = null)
    {
        try
        {
    _logger.LogInformation("Sending GET request to: {Uri}", uri);
     
   using var request = new HttpRequestMessage(HttpMethod.Get, uri);
     AddHeaders(request, headers);

     var response = await _httpClient.SendAsync(request);
   response.EnsureSuccessStatusCode();

    var content = await response.Content.ReadAsStringAsync();
   var contentType = response.Content.Headers.ContentType?.MediaType;
  var shouldValidateResponse = contentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) ?? false;
         
   content = ValidateResponseContent(content, shouldValidateResponse);
   _logger.LogInformation("GET request successful. Status: {StatusCode}, Content length: {Length}", 
  response.StatusCode, content.Length);
  
   return content;
  }
   catch (Exception ex)
    {
    _logger.LogError(ex, "Error sending GET request to: {Uri}", uri);
      throw;
  }
  }

    /// <summary>
    /// Sends a GET request to the specified URI
    /// </summary>
    /// <param name="uri">The URI to send the request to</param>
    /// <param name="headers">Optional headers to include in the request</param>
    /// <returns>The response content as a string</returns>
    public Task<string> GetAsync(string uri, RequestHeaders? headers)
  {
        return GetAsync(uri, headers?.ToDictionary());
    }

    /// <summary>
    /// Sends a POST request to the specified URI
    /// </summary>
    /// <param name="uri">The URI to send the request to</param>
    /// <param name="jsonBody">Optional JSON body to send with the request</param>
    /// <param name="headers">Optional headers to include in the request</param>
    /// <returns>The response content as a string</returns>
    public async Task<string> PostAsync(string uri, string? jsonBody = null, Dictionary<string, string>? headers = null)
    {
        try
        {
_logger.LogInformation("Sending POST request to: {Uri}", uri);

          using var request = new HttpRequestMessage(HttpMethod.Post, uri);
            AddHeaders(request, headers);
        
            if (!string.IsNullOrEmpty(jsonBody))
            {
      var validatedJson = ValidateAndFormatJson(jsonBody);
 request.Content = new StringContent(validatedJson, Encoding.UTF8, "application/json");
          _logger.LogDebug("POST body size: {Size} bytes", validatedJson.Length);
    }

    var response = await _httpClient.SendAsync(request);
   response.EnsureSuccessStatusCode();

       var content = await response.Content.ReadAsStringAsync();
    var contentType = response.Content.Headers.ContentType?.MediaType;
      var shouldValidateResponse = contentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) ?? false;
    
   content = ValidateResponseContent(content, shouldValidateResponse);
 _logger.LogInformation("POST request successful. Status: {StatusCode}, Content length: {Length}", 
    response.StatusCode, content.Length);
    
    return content;
      }
        catch (JsonException ex)
        {
   _logger.LogError(ex, "JSON validation error in POST request to: {Uri}", uri);
         throw;
    }
      catch (Exception ex)
     {
   _logger.LogError(ex, "Error sending POST request to: {Uri}", uri);
    throw;
        }
    }

    /// <summary>
    /// Sends a POST request to the specified URI
    /// </summary>
    /// <param name="uri">The URI to send the request to</param>
    /// <param name="jsonBody">Optional JSON body to send with the request</param>
    /// <param name="headers">Optional headers to include in the request</param>
    /// <returns>The response content as a string</returns>
  public Task<string> PostAsync(string uri, string? jsonBody, RequestHeaders? headers)
    {
     return PostAsync(uri, jsonBody, headers?.ToDictionary());
    }

    /// <summary>
    /// Sends a PUT request to the specified URI
    /// </summary>
    /// <param name="uri">The URI to send the request to</param>
    /// <param name="jsonBody">Optional JSON body to send with the request</param>
    /// <param name="headers">Optional headers to include in the request</param>
    /// <returns>The response content as a string</returns>
    public async Task<string> PutAsync(string uri, string? jsonBody = null, Dictionary<string, string>? headers = null)
    {
 try
        {
   _logger.LogInformation("Sending PUT request to: {Uri}", uri);
         
     using var request = new HttpRequestMessage(HttpMethod.Put, uri);
   AddHeaders(request, headers);

 if (!string.IsNullOrEmpty(jsonBody))
    {
    var validatedJson = ValidateAndFormatJson(jsonBody);
       request.Content = new StringContent(validatedJson, Encoding.UTF8, "application/json");
    _logger.LogDebug("PUT body size: {Size} bytes", validatedJson.Length);
       }

     var response = await _httpClient.SendAsync(request);
    response.EnsureSuccessStatusCode();

      var content = await response.Content.ReadAsStringAsync();
       var contentType = response.Content.Headers.ContentType?.MediaType;
     var shouldValidateResponse = contentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) ?? false;
  
  content = ValidateResponseContent(content, shouldValidateResponse);
   _logger.LogInformation("PUT request successful. Status: {StatusCode}, Content length: {Length}", 
 response.StatusCode, content.Length);
  
      return content;
 }
   catch (JsonException ex)
        {
   _logger.LogError(ex, "JSON validation error in PUT request to: {Uri}", uri);
       throw;
    }
    catch (Exception ex)
     {
       _logger.LogError(ex, "Error sending PUT request to: {Uri}", uri);
    throw;
  }
    }

 /// <summary>
    /// Sends a PUT request to the specified URI
  /// </summary>
    /// <param name="uri">The URI to send the request to</param>
    /// <param name="jsonBody">Optional JSON body to send with the request</param>
    /// <param name="headers">Optional headers to include in the request</param>
    /// <returns>The response content as a string</returns>
    public Task<string> PutAsync(string uri, string? jsonBody, RequestHeaders? headers)
    {
     return PutAsync(uri, jsonBody, headers?.ToDictionary());
    }

    /// <summary>
    /// Sends a PATCH request to the specified URI
    /// </summary>
    /// <param name="uri">The URI to send the request to</param>
    /// <param name="jsonBody">Optional JSON body to send with the request</param>
    /// <param name="headers">Optional headers to include in the request</param>
    /// <returns>The response content as a string</returns>
    public async Task<string> PatchAsync(string uri, string? jsonBody = null, Dictionary<string, string>? headers = null)
    {
 try
   {
   _logger.LogInformation("Sending PATCH request to: {Uri}", uri);
          
 using var request = new HttpRequestMessage(HttpMethod.Patch, uri);
    AddHeaders(request, headers);
  
    if (!string.IsNullOrEmpty(jsonBody))
   {
    var validatedJson = ValidateAndFormatJson(jsonBody);
       request.Content = new StringContent(validatedJson, Encoding.UTF8, "application/json");
  _logger.LogDebug("PATCH body size: {Size} bytes", validatedJson.Length);
  }

         var response = await _httpClient.SendAsync(request);
     response.EnsureSuccessStatusCode();

  var content = await response.Content.ReadAsStringAsync();
       var contentType = response.Content.Headers.ContentType?.MediaType;
       var shouldValidateResponse = contentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) ?? false;
     
 content = ValidateResponseContent(content, shouldValidateResponse);
 _logger.LogInformation("PATCH request successful. Status: {StatusCode}, Content length: {Length}", 
 response.StatusCode, content.Length);
            
    return content;
   }
   catch (JsonException ex)
   {
      _logger.LogError(ex, "JSON validation error in PATCH request to: {Uri}", uri);
   throw;
   }
    catch (Exception ex)
     {
  _logger.LogError(ex, "Error sending PATCH request to: {Uri}", uri);
       throw;
        }
    }

    /// <summary>
    /// Sends a PATCH request to the specified URI
    /// </summary>
    /// <param name="uri">The URI to send the request to</param>
    /// <param name="jsonBody">Optional JSON body to send with the request</param>
    /// <param name="headers">Optional headers to include in the request</param>
    /// <returns>The response content as a string</returns>
    public Task<string> PatchAsync(string uri, string? jsonBody, RequestHeaders? headers)
    {
      return PatchAsync(uri, jsonBody, headers?.ToDictionary());
    }

    /// <summary>
    /// Sends a DELETE request to the specified URI
    /// </summary>
    /// <param name="uri">The URI to send the request to</param>
    /// <param name="headers">Optional headers to include in the request</param>
  /// <returns>The response content as a string</returns>
    public async Task<string> DeleteAsync(string uri, Dictionary<string, string>? headers = null)
    {
  try
        {
       _logger.LogInformation("Sending DELETE request to: {Uri}", uri);
        
     using var request = new HttpRequestMessage(HttpMethod.Delete, uri);
    AddHeaders(request, headers);

  var response = await _httpClient.SendAsync(request);
  response.EnsureSuccessStatusCode();

   var content = await response.Content.ReadAsStringAsync();
       var contentType = response.Content.Headers.ContentType?.MediaType;
         var shouldValidateResponse = contentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) ?? false;
     
   content = ValidateResponseContent(content, shouldValidateResponse);
    _logger.LogInformation("DELETE request successful. Status: {StatusCode}, Content length: {Length}", 
 response.StatusCode, content.Length);
   
      return content;
 }
      catch (Exception ex)
{
     _logger.LogError(ex, "Error sending DELETE request to: {Uri}", uri);
 throw;
     }
    }

    /// <summary>
    /// Sends a DELETE request to the specified URI
    /// </summary>
 /// <param name="uri">The URI to send the request to</param>
    /// <param name="headers">Optional headers to include in the request</param>
    /// <returns>The response content as a string</returns>
    public Task<string> DeleteAsync(string uri, RequestHeaders? headers)
    {
      return DeleteAsync(uri, headers?.ToDictionary());
    }

    /// <summary>
    /// Sends a HEAD request to the specified URI
    /// </summary>
    /// <param name="uri">The URI to send the request to</param>
    /// <param name="headers">Optional headers to include in the request</param>
    /// <returns>The response headers as a dictionary</returns>
    public async Task<Dictionary<string, string>> HeadAsync(string uri, Dictionary<string, string>? headers = null)
    {
    try
        {
  _logger.LogInformation("Sending HEAD request to: {Uri}", uri);
       
      using var request = new HttpRequestMessage(HttpMethod.Head, uri);
   AddHeaders(request, headers);

  var response = await _httpClient.SendAsync(request);
      response.EnsureSuccessStatusCode();

   var responseHeaders = new Dictionary<string, string>();
     foreach (var header in response.Headers)
  {
  responseHeaders[header.Key] = string.Join(", ", header.Value);
}
  foreach (var header in response.Content.Headers)
      {
 responseHeaders[header.Key] = string.Join(", ", header.Value);
            }

     _logger.LogInformation("HEAD request successful. Status: {StatusCode}, Headers count: {Count}", 
        response.StatusCode, responseHeaders.Count);
       
    return responseHeaders;
        }
    catch (Exception ex)
    {
 _logger.LogError(ex, "Error sending HEAD request to: {Uri}", uri);
throw;
        }
    }

    /// <summary>
    /// Sends a HEAD request to the specified URI
    /// </summary>
    /// <param name="uri">The URI to send the request to</param>
    /// <param name="headers">Optional headers to include in the request</param>
    /// <returns>The response headers as RequestHeaders</returns>
    public async Task<RequestHeaders> HeadAsync(string uri, RequestHeaders? headers)
    {
     var result = await HeadAsync(uri, headers?.ToDictionary());
        return RequestHeaders.FromDictionary(result);
    }

    /// <summary>
    /// Sends an OPTIONS request to the specified URI
    /// </summary>
    /// <param name="uri">The URI to send the request to</param>
    /// <param name="headers">Optional headers to include in the request</param>
    /// <returns>The response content as a string</returns>
  public async Task<string> OptionsAsync(string uri, Dictionary<string, string>? headers = null)
    {
     try
     {
  _logger.LogInformation("Sending OPTIONS request to: {Uri}", uri);
     
  using var request = new HttpRequestMessage(HttpMethod.Options, uri);
AddHeaders(request, headers);

      var response = await _httpClient.SendAsync(request);
     response.EnsureSuccessStatusCode();

 var content = await response.Content.ReadAsStringAsync();
   var contentType = response.Content.Headers.ContentType?.MediaType;
     var shouldValidateResponse = contentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) ?? false;
       
       content = ValidateResponseContent(content, shouldValidateResponse);
    _logger.LogInformation("OPTIONS request successful. Status: {StatusCode}, Content length: {Length}", 
    response.StatusCode, content.Length);
  
   return content;
     }
    catch (Exception ex)
        {
    _logger.LogError(ex, "Error sending OPTIONS request to: {Uri}", uri);
     throw;
  }
    }

    /// <summary>
    /// Sends an OPTIONS request to the specified URI
    /// </summary>
/// <param name="uri">The URI to send the request to</param>
    /// <param name="headers">Optional headers to include in the request</param>
    /// <returns>The response content as a string</returns>
  public Task<string> OptionsAsync(string uri, RequestHeaders? headers)
  {
  return OptionsAsync(uri, headers?.ToDictionary());
    }

    /// <summary>
    /// Helper method to add headers to an HTTP request
    /// </summary>
    /// <param name="request">The HTTP request message</param>
    /// <param name="headers">Optional headers to add</param>
  private void AddHeaders(HttpRequestMessage request, Dictionary<string, string>? headers)
    {
        if (headers == null || headers.Count == 0)
            return;

   foreach (var header in headers)
        {
    try
{
          request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
 catch (Exception ex)
            {
           _logger.LogWarning(ex, "Failed to add header {Key}: {Value}", header.Key, header.Value);
            }
  }
    }

    /// <summary>
    /// Validates if a string is valid JSON
    /// </summary>
    /// <param name="json">The JSON string to validate</param>
    /// <returns>True if valid JSON, false otherwise</returns>
    private bool IsValidJson(string json)
    {
 if (string.IsNullOrWhiteSpace(json))
return false;

        try
        {
      JToken.Parse(json);
            return true;
        }
     catch (JsonReaderException ex)
        {
    _logger.LogWarning(ex, "Invalid JSON detected: {Json}", json.Length > 100 ? json.Substring(0, 100) + "..." : json);
return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error validating JSON");
            return false;
        }
    }

    /// <summary>
    /// Validates and formats JSON string
 /// </summary>
    /// <param name="json">The JSON string to validate and format</param>
    /// <returns>Validated JSON string</returns>
    /// <exception cref="JsonException">Thrown when JSON is invalid</exception>
    private string ValidateAndFormatJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
 throw new JsonException("JSON body cannot be null or empty");

      try
        {
            var parsedJson = JToken.Parse(json);
 _logger.LogDebug("JSON validation successful. Type: {Type}", parsedJson.Type);
       return parsedJson.ToString(Formatting.None);
        }
        catch (JsonReaderException ex)
        {
     _logger.LogError(ex, "JSON validation failed");
            throw new JsonException($"Invalid JSON format: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Validates response content as JSON
    /// </summary>
    /// <param name="content">The response content to validate</param>
    /// <param name="validateJson">Whether to validate the response as JSON</param>
    /// <returns>The validated content</returns>
    private string ValidateResponseContent(string content, bool validateJson = false)
    {
      if (validateJson && !string.IsNullOrWhiteSpace(content))
 {
       if (!IsValidJson(content))
   {
     _logger.LogWarning("Response content is not valid JSON");
      }
            else
            {
       _logger.LogDebug("Response JSON validation successful");
            }
     }
    return content;
    }
}
