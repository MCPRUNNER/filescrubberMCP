using System.ComponentModel;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using filescrubberMCP.Interfaces;
using filescrubberMCP.Models;

namespace filescrubberMCP.Tools;

/// <summary>
/// Provides MCP tools for URI/HTTP operations
/// </summary>
[McpServerToolType]
public class UriTools : IUriTools
{
    private readonly ILogger<UriTools> _logger;
    private readonly IUriService _uriService;

    /// <summary>
    /// Initializes a new instance of the UriTools class
    /// </summary>
    /// <param name="logger">Logger for the tools</param>
    /// <param name="uriService">Service for URI operations</param>
    public UriTools(
        ILogger<UriTools> logger,
        IUriService uriService)
    {
        _logger = logger;
        _uriService = uriService;
    }

    /// <summary>
    /// Sends a GET request to the specified URI
    /// </summary>
    [McpServerTool(Name = "fscrub_uri_get"), Description("Sends a GET request to the specified URI and returns the response content.")]
    public async Task<string> Get(string uri, string? headersJson = null)
    {
        try
        {
            Dictionary<string, string>? headers = null;
            if (!string.IsNullOrEmpty(headersJson))
            {
                headers = JsonConvert.DeserializeObject<Dictionary<string, string>>(headersJson);
            }

            var result = await _uriService.GetAsync(uri, headers);
            return JsonConvert.SerializeObject(new
            {
                success = true,
                content = result
            }, Formatting.Indented);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GET tool for URI: {Uri}", uri);
            return JsonConvert.SerializeObject(new
            {
                success = false,
                error = ex.Message
            }, Formatting.Indented);
        }
    }

    /// <summary>
    /// Sends a POST request to the specified URI
    /// </summary>
    [McpServerTool(Name = "fscrub_uri_post"), Description("Sends a POST request to the specified URI with optional JSON body and headers.")]
    public async Task<string> Post(string uri, string? jsonBody = null, string? headersJson = null)
    {
        try
        {
            Dictionary<string, string>? headers = null;
            if (!string.IsNullOrEmpty(headersJson))
            {
                headers = JsonConvert.DeserializeObject<Dictionary<string, string>>(headersJson);
            }

            var result = await _uriService.PostAsync(uri, jsonBody, headers);
            return JsonConvert.SerializeObject(new
            {
                success = true,
                content = result
            }, Formatting.Indented);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in POST tool for URI: {Uri}", uri);
            return JsonConvert.SerializeObject(new
            {
                success = false,
                error = ex.Message
            }, Formatting.Indented);
        }
    }

    /// <summary>
    /// Sends a PUT request to the specified URI
    /// </summary>
    [McpServerTool(Name = "fscrub_uri_put"), Description("Sends a PUT request to the specified URI with optional JSON body and headers.")]
    public async Task<string> Put(string uri, string? jsonBody = null, string? headersJson = null)
    {
        try
        {
            Dictionary<string, string>? headers = null;
            if (!string.IsNullOrEmpty(headersJson))
            {
                headers = JsonConvert.DeserializeObject<Dictionary<string, string>>(headersJson);
            }

            var result = await _uriService.PutAsync(uri, jsonBody, headers);
            return JsonConvert.SerializeObject(new
            {
                success = true,
                content = result
            }, Formatting.Indented);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in PUT tool for URI: {Uri}", uri);
            return JsonConvert.SerializeObject(new
            {
                success = false,
                error = ex.Message
            }, Formatting.Indented);
        }
    }

    /// <summary>
    /// Sends a PATCH request to the specified URI
    /// </summary>
    [McpServerTool(Name = "fscrub_uri_patch"), Description("Sends a PATCH request to the specified URI with optional JSON body and headers.")]
    public async Task<string> Patch(string uri, string? jsonBody = null, string? headersJson = null)
    {
        try
        {
            Dictionary<string, string>? headers = null;
            if (!string.IsNullOrEmpty(headersJson))
            {
                headers = JsonConvert.DeserializeObject<Dictionary<string, string>>(headersJson);
            }

            var result = await _uriService.PatchAsync(uri, jsonBody, headers);
            return JsonConvert.SerializeObject(new
            {
                success = true,
                content = result
            }, Formatting.Indented);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in PATCH tool for URI: {Uri}", uri);
            return JsonConvert.SerializeObject(new
            {
                success = false,
                error = ex.Message
            }, Formatting.Indented);
        }
    }

    /// <summary>
    /// Sends a DELETE request to the specified URI
    /// </summary>
    [McpServerTool(Name = "fscrub_uri_delete"), Description("Sends a DELETE request to the specified URI with optional headers.")]
    public async Task<string> Delete(string uri, string? headersJson = null)
    {
        try
        {
            Dictionary<string, string>? headers = null;
            if (!string.IsNullOrEmpty(headersJson))
            {
                headers = JsonConvert.DeserializeObject<Dictionary<string, string>>(headersJson);
            }

            var result = await _uriService.DeleteAsync(uri, headers);
            return JsonConvert.SerializeObject(new
            {
                success = true,
                content = result
            }, Formatting.Indented);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DELETE tool for URI: {Uri}", uri);
            return JsonConvert.SerializeObject(new
            {
                success = false,
                error = ex.Message
            }, Formatting.Indented);
        }
    }

    /// <summary>
    /// Sends a HEAD request to the specified URI
    /// </summary>
    [McpServerTool(Name = "fscrub_uri_head"), Description("Sends a HEAD request to the specified URI and returns the response headers.")]
    public async Task<string> Head(string uri, string? headersJson = null)
    {
        try
        {
            Dictionary<string, string>? headers = null;
            if (!string.IsNullOrEmpty(headersJson))
            {
                headers = JsonConvert.DeserializeObject<Dictionary<string, string>>(headersJson);
            }

            var result = await _uriService.HeadAsync(uri, headers);
            return JsonConvert.SerializeObject(new
            {
                success = true,
                headers = result
            }, Formatting.Indented);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in HEAD tool for URI: {Uri}", uri);
            return JsonConvert.SerializeObject(new
            {
                success = false,
                error = ex.Message
            }, Formatting.Indented);
        }
    }

    /// <summary>
    /// Sends an OPTIONS request to the specified URI
    /// </summary>
    [McpServerTool(Name = "fscrub_uri_options"), Description("Sends an OPTIONS request to the specified URI with optional headers.")]
    public async Task<string> Options(string uri, string? headersJson = null)
    {
        try
        {
            Dictionary<string, string>? headers = null;
            if (!string.IsNullOrEmpty(headersJson))
            {
                headers = JsonConvert.DeserializeObject<Dictionary<string, string>>(headersJson);
            }

            var result = await _uriService.OptionsAsync(uri, headers);
            return JsonConvert.SerializeObject(new
            {
                success = true,
                content = result
            }, Formatting.Indented);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OPTIONS tool for URI: {Uri}", uri);
            return JsonConvert.SerializeObject(new
            {
                success = false,
                error = ex.Message
            }, Formatting.Indented);
        }
    }
}
