using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using filescrubberMCP.Interfaces;

namespace filescrubberMCP.Tools;

/// <summary>
/// Provides MCP tools for sample functionality
/// </summary>
[McpServerToolType]
public class SampleTools : ISampleTools
{
    private readonly ILogger<SampleTools> _logger;
    private readonly ISampleService _sampleService;

    /// <summary>
    /// Initializes a new instance of the SampleTools class
    /// </summary>
    /// <param name="logger">Logger for the tools</param>
    /// <param name="sampleService">Service for sample operations</param>
    public SampleTools(
        ILogger<SampleTools> logger,
        ISampleService sampleService)
    {
        _logger = logger;
        _sampleService = sampleService;
    }

    /// <summary>
    /// Initialize the connection
    /// </summary>
    [McpServerTool(Name = "template_initialize_connection"), Description("Initialize the connection.")]
    public async Task<string> Initialize(string connectionName = "DefaultConnection")
    {
        try
        {
            var result = await _sampleService.InitializeConnectionAsync(connectionName);
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Initialize tool");
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = ex.Message
            }, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    /// <summary>
    /// Gets sample data
    /// </summary>
    [McpServerTool(Name = "template_get_sample_data"), Description("Gets sample data from the system.")]
    public async Task<string> GetSampleData(string connectionName = "DefaultConnection")
    {
        try
        {
            var result = await _sampleService.GetSampleDataAsync(connectionName);
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetSampleData tool");
            return JsonSerializer.Serialize(new
            {
                error = ex.Message
            }, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    /// <summary>
    /// Executes a sample operation
    /// </summary>
    [McpServerTool(Name = "template_execute_operation"), Description("Executes a sample operation.")]
    public async Task<string> ExecuteOperation(string operation, string parameters = "{}")
    {
        try
        {
            var result = await _sampleService.ExecuteOperationAsync(operation, parameters);
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ExecuteOperation tool");
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = ex.Message,
                operation = operation
            }, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
