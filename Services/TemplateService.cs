using Microsoft.Extensions.Logging;
using filescrubberMCP.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scriban;

namespace filescrubberMCP.Services;

/// <summary>
/// Service implementation for processing Scriban templates with JSON data
/// </summary>
public class TemplateService : ITemplateService
{
    private readonly ILogger<TemplateService> _logger;
    private readonly IFileService _fileService;

    /// <summary>
    /// Initializes a new instance of the TemplateService class
    /// </summary>
    /// <param name="logger">Logger for the service</param>
    /// <param name="fileService">File service for file operations</param>
    public TemplateService(
        ILogger<TemplateService> logger,
        IFileService fileService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
    }

    /// <summary>
    /// Processes a Scriban template file with JSON data and saves the result to an output file
    /// </summary>
    /// <param name="templateFilePath">Path to the .sbn template file</param>
    /// <param name="jsonData">JSON data as string or object to be passed to the template</param>
    /// <param name="outputFilePath">Path where the processed template output will be saved</param>
    /// <returns>A task representing the asynchronous operation with the output file path on success, or error message on failure</returns>
    public async Task<string> ProcessTemplateAsync(string templateFilePath, object jsonData, string outputFilePath)
    {
        try
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(templateFilePath))
            {
                var error = "Template file path cannot be null or empty";
                _logger.LogError(error);
                return error;
            }

            if (jsonData == null)
            {
                var error = "JSON data cannot be null";
                _logger.LogError(error);
                return error;
            }

            if (string.IsNullOrWhiteSpace(outputFilePath))
            {
                var error = "Output file path cannot be null or empty";
                _logger.LogError(error);
                return error;
            }

            _logger.LogInformation("Processing Scriban template '{TemplatePath}' with JSON data to output '{OutputPath}'",
                templateFilePath, outputFilePath);

            // Convert JSON data to JObject
            var dataObject = ConvertToJObject(jsonData);
            _logger.LogDebug("Successfully converted JSON data with {PropertyCount} properties", dataObject.Properties().Count());

            // Read template content
            string templateText;
            try
            {
                templateText = await _fileService.ReadFileAsync(templateFilePath);
            }
            catch (FileNotFoundException ex)
            {
                var error = $"Template file not found: {templateFilePath}";
                _logger.LogError(ex, error);
                return error;
            }
            if (string.IsNullOrWhiteSpace(templateText))
            {
                var error = $"Template file is empty: {templateFilePath}";
                _logger.LogError(error);
                return error;
            }

            _logger.LogDebug("Template file loaded with {CharacterCount} characters", templateText.Length);

            // Parse Scriban template
            var template = Template.Parse(templateText);
            if (template.HasErrors)
            {
                var errorMessages = string.Join(Environment.NewLine, template.Messages.Select(m => $"- {m}"));
                _logger.LogError("Template parsing failed with {ErrorCount} errors:\n{ErrorMessages}",
                    template.Messages.Count, errorMessages);
                return $"Template parsing failed:\n{errorMessages}";
            }

            _logger.LogDebug("Scriban template parsed successfully");

            // Convert JObject to Dictionary for Scriban compatibility
            var dataDict = dataObject.ToObject<Dictionary<string, object>>() ?? new Dictionary<string, object>();
            _logger.LogDebug("Converted JSON data to dictionary with {KeyCount} keys", dataDict.Keys.Count);

            // Render template with data
            string result;
            try
            {
                result = await template.RenderAsync(dataDict);
                _logger.LogDebug("Template rendered successfully with {OutputLength} characters", result.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during template rendering");
                return $"Error during template rendering: {ex.Message}";
            }

            // Write result to output file
            try
            {
                await _fileService.WriteFileAsync(outputFilePath, result);
                _logger.LogInformation("Successfully processed Scriban template '{TemplatePath}' and saved to '{OutputPath}' ({OutputSize} bytes)",
                    templateFilePath, outputFilePath, result.Length);
                return outputFilePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing to output file: {OutputPath}", outputFilePath);
                return $"Error writing to output file: {ex.Message}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing Scriban template '{TemplatePath}': {Message}",
                templateFilePath, ex.Message);
            return $"Unexpected error processing Scriban template: {ex.Message}";
        }
    }

    /// <summary>
    /// Processes a Scriban template file with JSON data and returns the result as a string
    /// </summary>
    /// <param name="templateFilePath">Path to the .sbn template file</param>
    /// <param name="jsonData">JSON data as string or object to be passed to the template</param>
    /// <returns>A task representing the asynchronous operation with the rendered output as a string</returns>
    public async Task<string> RenderTemplateAsync(string templateFilePath, object jsonData)
    {
        try
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(templateFilePath))
            {
                var error = "Template file path cannot be null or empty";
                _logger.LogError(error);
                throw new ArgumentException(error, nameof(templateFilePath));
            }

            if (jsonData == null)
            {
                var error = "JSON data cannot be null";
                _logger.LogError(error);
                throw new ArgumentNullException(nameof(jsonData), error);
            }

            _logger.LogInformation("Rendering Scriban template '{TemplatePath}' with JSON data", templateFilePath);

            // Convert JSON data to JObject
            var dataObject = ConvertToJObject(jsonData);
            _logger.LogDebug("Successfully converted JSON data with {PropertyCount} properties", dataObject.Properties().Count());

            // Read template content
            string templateText;
            try
            {
                templateText = await _fileService.ReadFileAsync(templateFilePath);
            }
            catch (FileNotFoundException ex)
            {
                var error = $"Template file not found: {templateFilePath}";
                _logger.LogError(ex, error);
                throw;
            }
            if (string.IsNullOrWhiteSpace(templateText))
            {
                var error = $"Template file is empty: {templateFilePath}";
                _logger.LogError(error);
                throw new InvalidOperationException(error);
            }

            _logger.LogDebug("Template file loaded with {CharacterCount} characters", templateText.Length);

            // Parse Scriban template
            var template = Template.Parse(templateText);
            if (template.HasErrors)
            {
                var errorMessages = string.Join(Environment.NewLine, template.Messages.Select(m => $"- {m}"));
                _logger.LogError("Template parsing failed with {ErrorCount} errors:\n{ErrorMessages}",
                    template.Messages.Count, errorMessages);
                throw new InvalidOperationException($"Template parsing failed:\n{errorMessages}");
            }

            _logger.LogDebug("Scriban template parsed successfully");

            // Convert JObject to Dictionary for Scriban compatibility
            var dataDict = dataObject.ToObject<Dictionary<string, object>>() ?? new Dictionary<string, object>();
            _logger.LogDebug("Converted JSON data to dictionary with {KeyCount} keys", dataDict.Keys.Count);

            // Render template with data
            string result;
            try
            {
                result = await template.RenderAsync(dataDict);
                _logger.LogDebug("Template rendered successfully with {OutputLength} characters", result.Length);
                _logger.LogInformation("Successfully rendered Scriban template '{TemplatePath}'", templateFilePath);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during template rendering");
                throw new InvalidOperationException($"Error during template rendering: {ex.Message}", ex);
            }
        }
        catch (Exception ex) when (!(ex is ArgumentException || ex is ArgumentNullException || ex is FileNotFoundException || ex is InvalidOperationException))
        {
            _logger.LogError(ex, "Unexpected error rendering Scriban template '{TemplatePath}': {Message}",
                templateFilePath, ex.Message);
            throw new InvalidOperationException($"Unexpected error rendering Scriban template: {ex.Message}", ex);
        }
    }

    #region Helper Methods

    /// <summary>
    /// Converts various input types to JObject for template processing
    /// </summary>
    /// <param name="input">Input data (string, JObject, JToken, or other object)</param>
    /// <returns>JObject representation of the input data</returns>
    /// <exception cref="JsonException">Thrown when JSON parsing fails</exception>
    private JObject ConvertToJObject(object input)
    {
        try
        {
            return input switch
            {
                JObject jObj => jObj,
                JToken jToken => jToken as JObject ?? throw new JsonException("JToken is not a JObject"),
                string jsonString when !string.IsNullOrWhiteSpace(jsonString) => JObject.Parse(jsonString),
                string => throw new JsonException("JSON string cannot be null or empty"),
                _ => JObject.FromObject(input)
            };
        }
        catch (JsonReaderException ex)
        {
            throw new JsonException($"Invalid JSON format: {ex.Message}", ex);
        }
        catch (Exception ex) when (!(ex is JsonException))
        {
            throw new JsonException($"Failed to convert input to JSON object: {ex.Message}", ex);
        }
    }

    #endregion
}
