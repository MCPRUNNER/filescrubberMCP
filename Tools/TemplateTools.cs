using System.ComponentModel;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using filescrubberMCP.Interfaces;

namespace filescrubberMCP.Tools;

/// <summary>
/// Provides MCP tools for Scriban template processing functionality
/// </summary>
[McpServerToolType]
public class TemplateTools : ITemplateTools
{
    private readonly ILogger<TemplateTools> _logger;
    private readonly ITemplateService _templateService;

    /// <summary>
    /// Initializes a new instance of the TemplateTools class
    /// </summary>
    /// <param name="logger">Logger for the tools</param>
    /// <param name="templateService">Service for template operations</param>
    public TemplateTools(
        ILogger<TemplateTools> logger,
        ITemplateService templateService)
    {
        _logger = logger;
        _templateService = templateService;
    }

    /// <summary>
    /// Processes a Scriban template file with JSON data and saves the result to an output file
    /// </summary>
    [McpServerTool(Name = "fscrub_scriban_process_template"), Description("Processes a Scriban template file (.sbn) with JSON data and saves the rendered result to an output file. Returns the output file path on success or error message on failure.")]
    public async Task<string> ProcessTemplate(
        [Description("Path to the .sbn template file (relative to workspace root or absolute path)")]
        string templateFilePath,
        [Description("JSON data as a string to be passed to the template for rendering")]
        string jsonData,
        [Description("Path where the processed template output will be saved (relative to workspace root or absolute path)")]
        string outputFilePath)
    {
        try
        {
            _logger.LogInformation("ProcessTemplate tool called with template: {TemplatePath}, output: {OutputPath}",
                templateFilePath, outputFilePath);

            var result = await _templateService.ProcessTemplateAsync(templateFilePath, jsonData, outputFilePath);

            // Check if result is an error message or success path
            var isError = result.Contains("error", StringComparison.OrdinalIgnoreCase) ||
                          result.Contains("cannot", StringComparison.OrdinalIgnoreCase) ||
                          result.Contains("failed", StringComparison.OrdinalIgnoreCase) ||
                          result.Contains("not found", StringComparison.OrdinalIgnoreCase);

            if (isError)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    error = result,
                    templateFilePath,
                    outputFilePath
                }, Formatting.Indented);
            }
            else
            {
                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    outputFilePath = result,
                    message = $"Template processed successfully and saved to {result}"
                }, Formatting.Indented);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ProcessTemplate tool");
            return JsonConvert.SerializeObject(new
            {
                success = false,
                error = ex.Message,
                templateFilePath,
                outputFilePath
            }, Formatting.Indented);
        }
    }

    /// <summary>
    /// Processes a Scriban template file with JSON data and returns the rendered result
    /// </summary>
    [McpServerTool(Name = "fscrub_scriban_render_template"), Description("Processes a Scriban template file (.sbn) with JSON data and returns the rendered result as a string without saving to a file.")]
    public async Task<string> RenderTemplate(
        [Description("Path to the .sbn template file (relative to workspace root or absolute path)")]
        string templateFilePath,
        [Description("JSON data as a string to be passed to the template for rendering")]
        string jsonData)
    {
        try
        {
            _logger.LogInformation("RenderTemplate tool called with template: {TemplatePath}", templateFilePath);

            var result = await _templateService.RenderTemplateAsync(templateFilePath, jsonData);

            return JsonConvert.SerializeObject(new
            {
                success = true,
                renderedOutput = result,
                templateFilePath,
                outputLength = result.Length
            }, Formatting.Indented);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in RenderTemplate tool");
            return JsonConvert.SerializeObject(new
            {
                success = false,
                error = ex.Message,
                templateFilePath
            }, Formatting.Indented);
        }
    }
}
