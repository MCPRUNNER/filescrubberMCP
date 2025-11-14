using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using filescrubberMCP.Interfaces;
using filescrubberMCP.Models;

namespace filescrubberMCP.Services;

/// <summary>
/// Service implementation for executing workflows with sequential steps
/// </summary>
public class WorkflowService : IWorkflowService
{
    private readonly ILogger<WorkflowService> _logger;
    private readonly IFileService _fileService;
    private readonly IUriService _uriService;
    private readonly ITemplateService _templateService;
    private readonly IParserService _parserService;
    private readonly IAIService _aiService;

    /// <summary>
    /// Initializes a new instance of the WorkflowService class
    /// </summary>
    public WorkflowService(
        ILogger<WorkflowService> logger,
        IFileService fileService,
        IUriService uriService,
        ITemplateService templateService,
        IParserService parserService,
        IAIService aiService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _uriService = uriService ?? throw new ArgumentNullException(nameof(uriService));
        _templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
        _parserService = parserService ?? throw new ArgumentNullException(nameof(parserService));
        _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
    }

    /// <summary>
    /// Loads a workflow definition from a JSON file
    /// </summary>
    public async Task<WorkflowDefinition> LoadWorkflowAsync(string workflowFilePath)
    {
        try
        {
            _logger.LogInformation("Loading workflow from file: {WorkflowFilePath}", workflowFilePath);

            var content = await _fileService.ReadFileAsync(workflowFilePath);
            var workflow = JsonConvert.DeserializeObject<WorkflowDefinition>(content);

            if (workflow == null)
            {
                throw new InvalidOperationException($"Failed to deserialize workflow from file: {workflowFilePath}");
            }

            _logger.LogInformation("Successfully loaded workflow with {StepCount} steps", workflow.Steps.Count);
            return workflow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading workflow from file: {WorkflowFilePath}", workflowFilePath);
            throw;
        }
    }

    /// <summary>
    /// Executes a workflow with sequential steps
    /// </summary>
    public async Task<WorkflowResult> ExecuteWorkflowAsync(WorkflowDefinition workflow)
    {
        var result = new WorkflowResult { Success = true };
        var context = new Dictionary<string, object>();

        _logger.LogInformation("Starting workflow execution with {StepCount} steps", workflow.Steps.Count);

        try
        {
            foreach (var step in workflow.Steps)
            {
                if (!step.Enabled)
                {
                    _logger.LogInformation("Skipping disabled step: {StepName}", step.Name);
                    continue;
                }

                _logger.LogInformation("Executing step: {StepName} ({StepType})", step.Name, step.Type);
                var stopwatch = Stopwatch.StartNew();

                try
                {
                    // Resolve parameter placeholders with context values
                    var resolvedParameters = ResolveParameters(step.Parameters, context);

                    // Execute the step based on its type
                    var stepOutput = await ExecuteStepAsync(step.Type, resolvedParameters);
                    stopwatch.Stop();

                    // Store the output in context
                    if (step.Output != null && !string.IsNullOrEmpty(step.Output.Name))
                    {
                        var outputValue = ExtractOutputValue(stepOutput, step.Output);
                        context[$"{step.Name}.{step.Output.Name}"] = outputValue;
                        _logger.LogDebug("Stored output: {StepName}.{OutputName}", step.Name, step.Output.Name);
                    }
                    else
                    {
                        // Store entire output under step name
                        context[step.Name] = stepOutput;
                    }

                    // Also store resolved parameters for reference (e.g., {StepName.parameterName})
                    foreach (var param in resolvedParameters)
                    {
                        context[$"{step.Name}.{param.Key}"] = param.Value;
                        _logger.LogDebug("Stored parameter: {StepName}.{ParamKey} = {ParamValue}",
                            step.Name, param.Key, param.Value);
                    }

                    result.StepOutputs[step.Name] = stepOutput;
                    result.StepResults.Add(new WorkflowStepResult
                    {
                        StepName = step.Name,
                        Success = true,
                        Output = stepOutput,
                        ExecutionTimeMs = stopwatch.ElapsedMilliseconds
                    });

                    _logger.LogInformation("Step completed successfully: {StepName} (took {ElapsedMs}ms)",
                        step.Name, stopwatch.ElapsedMilliseconds);
                }
                catch (Exception stepEx)
                {
                    stopwatch.Stop();
                    _logger.LogError(stepEx, "Error executing step: {StepName}", step.Name);

                    result.Success = false;
                    result.ErrorMessage = $"Step '{step.Name}' failed: {stepEx.Message}";
                    result.StepResults.Add(new WorkflowStepResult
                    {
                        StepName = step.Name,
                        Success = false,
                        ErrorMessage = stepEx.Message,
                        ExecutionTimeMs = stopwatch.ElapsedMilliseconds
                    });

                    // Stop execution on error
                    break;
                }
            }

            if (result.Success)
            {
                _logger.LogInformation("Workflow execution completed successfully");
            }
            else
            {
                _logger.LogWarning("Workflow execution failed: {ErrorMessage}", result.ErrorMessage);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during workflow execution");
            result.Success = false;
            result.ErrorMessage = $"Workflow execution failed: {ex.Message}";
            return result;
        }
    }

    /// <summary>
    /// Loads and executes a workflow from a JSON file
    /// </summary>
    public async Task<WorkflowResult> ExecuteWorkflowFromFileAsync(string workflowFilePath)
    {
        try
        {
            var workflow = await LoadWorkflowAsync(workflowFilePath);
            return await ExecuteWorkflowAsync(workflow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing workflow from file: {WorkflowFilePath}", workflowFilePath);
            return new WorkflowResult
            {
                Success = false,
                ErrorMessage = $"Failed to load or execute workflow: {ex.Message}"
            };
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// Resolves parameter placeholders with context values
    /// </summary>
    private Dictionary<string, object> ResolveParameters(
        Dictionary<string, object> parameters,
        Dictionary<string, object> context)
    {
        var resolved = new Dictionary<string, object>();

        foreach (var kvp in parameters)
        {
            if (kvp.Value is string strValue)
            {
                // Replace placeholders like {StepName.OutputName}
                var resolvedValue = ResolvePlaceholders(strValue, context);
                resolved[kvp.Key] = resolvedValue;
            }
            else if (kvp.Value is Dictionary<string, object> nestedDict)
            {
                // Recursively resolve nested dictionaries
                var resolvedNested = new Dictionary<string, object>();
                foreach (var nestedKvp in nestedDict)
                {
                    if (nestedKvp.Value is string nestedStr)
                    {
                        resolvedNested[nestedKvp.Key] = ResolvePlaceholders(nestedStr, context);
                    }
                    else
                    {
                        resolvedNested[nestedKvp.Key] = nestedKvp.Value;
                    }
                }
                resolved[kvp.Key] = resolvedNested;
            }
            else if (kvp.Value is JObject jobj)
            {
                // Handle JSON.NET JObject (from deserialization)
                var resolvedNested = new Dictionary<string, object>();
                foreach (var prop in jobj.Properties())
                {
                    if (prop.Value is JValue jval && jval.Value is string nestedStr)
                    {
                        resolvedNested[prop.Name] = ResolvePlaceholders(nestedStr, context);
                    }
                    else
                    {
                        resolvedNested[prop.Name] = prop.Value;
                    }
                }
                resolved[kvp.Key] = resolvedNested;
            }
            else
            {
                resolved[kvp.Key] = kvp.Value;
            }
        }

        return resolved;
    }

    /// <summary>
    /// Resolves placeholders in a string value with context values
    /// </summary>
    private string ResolvePlaceholders(string value, Dictionary<string, object> context)
    {
        // Pattern to match {StepName.PropertyName} or {StepName}
        var pattern = @"\{([^}]+)\}";
        var matches = Regex.Matches(value, pattern);

        var result = value;
        foreach (Match match in matches)
        {
            var placeholder = match.Groups[1].Value;

            if (context.TryGetValue(placeholder, out var contextValue))
            {
                var replacementValue = contextValue?.ToString() ?? string.Empty;
                result = result.Replace(match.Value, replacementValue);
                _logger.LogDebug("Resolved placeholder {Placeholder} to: {Value}", match.Value, replacementValue);
            }
            else
            {
                _logger.LogWarning("Placeholder not found in context: {Placeholder}", placeholder);
            }
        }

        return result;
    }

    /// <summary>
    /// Extracts the output value based on output configuration
    /// </summary>
    private object ExtractOutputValue(object stepOutput, WorkflowStepOutput outputConfig)
    {
        if (outputConfig.Format?.Equals("JSON", StringComparison.OrdinalIgnoreCase) == true)
        {
            // Try to parse as JSON if the output is a string
            if (stepOutput is string jsonString)
            {
                try
                {
                    var parsed = JToken.Parse(jsonString);
                    return parsed;
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to parse output as JSON, returning as-is");
                    return stepOutput;
                }
            }
        }
        else if (outputConfig.Format?.Equals("XML", StringComparison.OrdinalIgnoreCase) == true)
        {
            // If output is a JSON array containing XML strings, convert it to proper XML
            if (stepOutput is string outputString)
            {
                try
                {
                    var parsed = JToken.Parse(outputString);
                    if (parsed is JArray array)
                    {
                        // Wrap multiple XML elements in a root element
                        var xmlDoc = new XDocument(new XElement("Results"));
                        foreach (var item in array)
                        {
                            var xmlString = item.ToString();
                            try
                            {
                                var element = XElement.Parse(xmlString);
                                xmlDoc.Root?.Add(element);
                            }
                            catch (System.Xml.XmlException ex)
                            {
                                // If not valid XML, add as a text element
                                _logger.LogWarning(ex, "Failed to parse item as XML. Adding as text element.");
                                xmlDoc.Root?.Add(new XElement("Result", xmlString));
                            }
                        }
                        return xmlDoc.ToString();
                    }
                    // If it's a single JSON value containing XML, just return it
                    return outputString;
                }
                catch (JsonException)
                {
                    // Not JSON, assume it's already XML
                    return stepOutput;
                }
            }
        }

        return stepOutput;
    }

    /// <summary>
    /// Executes a step based on its type
    /// </summary>
    private async Task<object> ExecuteStepAsync(string stepType, Dictionary<string, object> parameters)
    {
        // Remove the # prefix and trim whitespace
        var type = stepType.TrimStart('#').Trim();

        return type switch
        {
            "fscrub_uri_get" => await ExecuteUriGetAsync(parameters),
            "fscrub_uri_post" => await ExecuteUriPostAsync(parameters),
            "fscrub_uri_put" => await ExecuteUriPutAsync(parameters),
            "fscrub_uri_delete" => await ExecuteUriDeleteAsync(parameters),
            "fscrub_uri_patch" => await ExecuteUriPatchAsync(parameters),
            "fscrub_uri_head" => await ExecuteUriHeadAsync(parameters),
            "fscrub_uri_options" => await ExecuteUriOptionsAsync(parameters),
            "fscrub_file_read" => await ExecuteFileReadAsync(parameters),
            "fscrub_file_write" => await ExecuteFileWriteAsync(parameters),
            "fscrub_file_list" => await ExecuteFileListAsync(parameters),
            "fscrub_scriban_process_template" => await ExecuteTemplateProcessAsync(parameters),
            "fscrub_scriban_render_template" => await ExecuteTemplateRenderAsync(parameters),
            "fscrub_parser_search_json" => await ExecuteParserSearchJsonAsync(parameters),
            "fscrub_parser_search_xml" => await ExecuteParserSearchXmlAsync(parameters),
            "fscrub_parser_search_yaml" => await ExecuteParserSearchYamlAsync(parameters),
            "fscrub_parser_search_csv" => await ExecuteParserSearchCsvAsync(parameters),
            "fscrub_parser_search_excel" => await ExecuteParserSearchExcelAsync(parameters),
            "fscrub_parser_transform_xml" => await ExecuteParserTransformXmlAsync(parameters),
            "fscrub_ask_github_copilot" => await ExecuteAskGithubCopilotAsync(parameters),
            _ => throw new NotSupportedException($"Step type not supported: {stepType}")
        };
    }

    #region URI Operations

    private async Task<object> ExecuteUriGetAsync(Dictionary<string, object> parameters)
    {
        var uri = GetRequiredParameter<string>(parameters, "Uri", "uri");
        var headersJson = GetOptionalParameter<string>(parameters, "headersJson");

        Dictionary<string, string>? headers = null;
        if (!string.IsNullOrEmpty(headersJson))
        {
            headers = JsonConvert.DeserializeObject<Dictionary<string, string>>(headersJson);
        }

        return await _uriService.GetAsync(uri, headers);
    }

    private async Task<object> ExecuteUriPostAsync(Dictionary<string, object> parameters)
    {
        var uri = GetRequiredParameter<string>(parameters, "Uri", "uri");
        var jsonBody = GetOptionalParameter<string>(parameters, "jsonBody");
        var headersJson = GetOptionalParameter<string>(parameters, "headersJson");

        Dictionary<string, string>? headers = null;
        if (!string.IsNullOrEmpty(headersJson))
        {
            headers = JsonConvert.DeserializeObject<Dictionary<string, string>>(headersJson);
        }

        return await _uriService.PostAsync(uri, jsonBody, headers);
    }

    private async Task<object> ExecuteUriPutAsync(Dictionary<string, object> parameters)
    {
        var uri = GetRequiredParameter<string>(parameters, "Uri", "uri");
        var jsonBody = GetOptionalParameter<string>(parameters, "jsonBody");
        var headersJson = GetOptionalParameter<string>(parameters, "headersJson");

        Dictionary<string, string>? headers = null;
        if (!string.IsNullOrEmpty(headersJson))
        {
            headers = JsonConvert.DeserializeObject<Dictionary<string, string>>(headersJson);
        }

        return await _uriService.PutAsync(uri, jsonBody, headers);
    }

    private async Task<object> ExecuteUriDeleteAsync(Dictionary<string, object> parameters)
    {
        var uri = GetRequiredParameter<string>(parameters, "Uri", "uri");
        var headersJson = GetOptionalParameter<string>(parameters, "headersJson");

        Dictionary<string, string>? headers = null;
        if (!string.IsNullOrEmpty(headersJson))
        {
            headers = JsonConvert.DeserializeObject<Dictionary<string, string>>(headersJson);
        }

        return await _uriService.DeleteAsync(uri, headers);
    }

    private async Task<object> ExecuteUriPatchAsync(Dictionary<string, object> parameters)
    {
        var uri = GetRequiredParameter<string>(parameters, "Uri", "uri");
        var jsonBody = GetOptionalParameter<string>(parameters, "jsonBody");
        var headersJson = GetOptionalParameter<string>(parameters, "headersJson");

        Dictionary<string, string>? headers = null;
        if (!string.IsNullOrEmpty(headersJson))
        {
            headers = JsonConvert.DeserializeObject<Dictionary<string, string>>(headersJson);
        }

        return await _uriService.PatchAsync(uri, jsonBody, headers);
    }

    private async Task<object> ExecuteUriHeadAsync(Dictionary<string, object> parameters)
    {
        var uri = GetRequiredParameter<string>(parameters, "Uri", "uri");
        var headersJson = GetOptionalParameter<string>(parameters, "headersJson");

        Dictionary<string, string>? headers = null;
        if (!string.IsNullOrEmpty(headersJson))
        {
            headers = JsonConvert.DeserializeObject<Dictionary<string, string>>(headersJson);
        }

        return await _uriService.HeadAsync(uri, headers);
    }

    private async Task<object> ExecuteUriOptionsAsync(Dictionary<string, object> parameters)
    {
        var uri = GetRequiredParameter<string>(parameters, "Uri", "uri");
        var headersJson = GetOptionalParameter<string>(parameters, "headersJson");

        Dictionary<string, string>? headers = null;
        if (!string.IsNullOrEmpty(headersJson))
        {
            headers = JsonConvert.DeserializeObject<Dictionary<string, string>>(headersJson);
        }

        return await _uriService.OptionsAsync(uri, headers);
    }

    #endregion

    #region File Operations

    private async Task<object> ExecuteFileReadAsync(Dictionary<string, object> parameters)
    {
        var filePath = GetRequiredParameter<string>(parameters, "filePath");
        return await _fileService.ReadFileAsync(filePath);
    }

    private async Task<object> ExecuteFileWriteAsync(Dictionary<string, object> parameters)
    {
        var filePath = GetRequiredParameter<string>(parameters, "filePath");
        var content = GetRequiredParameter<string>(parameters, "content");
        await _fileService.WriteFileAsync(filePath, content);
        return $"Successfully wrote to file: {filePath}";
    }

    private async Task<object> ExecuteFileListAsync(Dictionary<string, object> parameters)
    {
        var directoryPath = GetRequiredParameter<string>(parameters, "directoryPath");
        var searchPattern = GetOptionalParameter<string>(parameters, "searchPattern") ?? "*";
        var recursive = GetOptionalParameter<bool>(parameters, "recursive", true);

        return await _fileService.ListFilesAsync(directoryPath, searchPattern, recursive);
    }

    #endregion

    #region Template Operations

    private async Task<object> ExecuteTemplateProcessAsync(Dictionary<string, object> parameters)
    {
        var templateFilePath = GetRequiredParameter<string>(parameters, "templateFilePath");
        var jsonData = GetRequiredParameter<string>(parameters, "jsonData");
        var outputFilePath = GetRequiredParameter<string>(parameters, "outputFilePath");

        return await _templateService.ProcessTemplateAsync(templateFilePath, jsonData, outputFilePath);
    }

    private async Task<object> ExecuteTemplateRenderAsync(Dictionary<string, object> parameters)
    {
        var templateFilePath = GetRequiredParameter<string>(parameters, "templateFilePath");
        var jsonData = GetRequiredParameter<string>(parameters, "jsonData");

        return await _templateService.RenderTemplateAsync(templateFilePath, jsonData);
    }

    #endregion

    #region Parser Operations

    private async Task<object> ExecuteParserSearchJsonAsync(Dictionary<string, object> parameters)
    {
        var jsonFilePath = GetRequiredParameter<string>(parameters, "jsonFilePath", "filePath");
        var jsonPath = GetRequiredParameter<string>(parameters, "jsonPath");
        var indented = GetOptionalParameter<bool>(parameters, "indented", true);
        var showKeyPaths = GetOptionalParameter<bool>(parameters, "showKeyPaths", false);

        var result = await Task.Run(() => _parserService.SearchJsonFile(jsonFilePath, jsonPath, indented, showKeyPaths));
        return result ?? string.Empty;
    }

    private async Task<object> ExecuteParserSearchXmlAsync(Dictionary<string, object> parameters)
    {
        var xmlFilePath = GetRequiredParameter<string>(parameters, "xmlFilePath", "filePath");
        var xPath = GetRequiredParameter<string>(parameters, "xPath");
        var indented = GetOptionalParameter<bool>(parameters, "indented", true);
        var showKeyPaths = GetOptionalParameter<bool>(parameters, "showKeyPaths", false);

        var result = await Task.Run(() => _parserService.SearchXmlFile(xmlFilePath, xPath, indented, showKeyPaths));
        return result ?? string.Empty;
    }

    private async Task<object> ExecuteParserSearchYamlAsync(Dictionary<string, object> parameters)
    {
        var yamlFilePath = GetRequiredParameter<string>(parameters, "yamlFilePath", "filePath");
        var jsonPath = GetRequiredParameter<string>(parameters, "jsonPath");
        var indented = GetOptionalParameter<bool>(parameters, "indented", true);
        var showKeyPaths = GetOptionalParameter<bool>(parameters, "showKeyPaths", false);

        var result = await Task.Run(() => _parserService.SearchYamlFile(yamlFilePath, jsonPath, indented, showKeyPaths));
        return result ?? string.Empty;
    }

    private async Task<object> ExecuteParserSearchCsvAsync(Dictionary<string, object> parameters)
    {
        var csvFilePath = GetRequiredParameter<string>(parameters, "csvFilePath", "filePath");
        var jsonPath = GetRequiredParameter<string>(parameters, "jsonPath");
        var hasHeaderRecord = GetOptionalParameter<bool>(parameters, "hasHeaderRecord", true);
        var ignoreBlankLines = GetOptionalParameter<bool>(parameters, "ignoreBlankLines", true);

        var result = await Task.Run(() => _parserService.SearchCsvFile(csvFilePath, jsonPath, hasHeaderRecord, ignoreBlankLines));
        return result ?? string.Empty;
    }

    private async Task<object> ExecuteParserSearchExcelAsync(Dictionary<string, object> parameters)
    {
        var excelFilePath = GetRequiredParameter<string>(parameters, "excelFilePath", "filePath");
        var jsonPath = GetRequiredParameter<string>(parameters, "jsonPath");

        var result = await Task.Run(() => _parserService.SearchExcelFile(excelFilePath, jsonPath));
        return result ?? string.Empty;
    }

    private async Task<object> ExecuteParserTransformXmlAsync(Dictionary<string, object> parameters)
    {
        var xmlFilePath = GetRequiredParameter<string>(parameters, "xmlFilePath");
        var xsltFilePath = GetRequiredParameter<string>(parameters, "xsltFilePath");
        var destinationFilePath = GetOptionalParameter<string>(parameters, "destinationFilePath");

        Dictionary<string, string>? xsltParameters = null;
        if (parameters.TryGetValue("xsltParameters", out var xsltParamsObj) && xsltParamsObj != null)
        {
            _logger.LogInformation("Found xsltParameters object of type: {Type}", xsltParamsObj.GetType().Name);

            if (xsltParamsObj is Dictionary<string, object> paramsDict)
            {
                xsltParameters = paramsDict.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.ToString()?.Trim() ?? string.Empty
                );
                _logger.LogInformation("Converted {Count} parameters from Dictionary<string, object>", xsltParameters.Count);
                foreach (var kvp in xsltParameters)
                {
                    _logger.LogInformation("  Parameter: {Key} = '{Value}'", kvp.Key, kvp.Value);
                }
            }
            else if (xsltParamsObj is string paramsJson)
            {
                xsltParameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(paramsJson);
                _logger.LogInformation("Deserialized {Count} parameters from JSON string", xsltParameters?.Count ?? 0);
            }
        }
        else
        {
            _logger.LogInformation("No xsltParameters found in parameters dictionary");
        }

        var result = await Task.Run(() => _parserService.TransformXmlWithXslt(xmlFilePath, xsltFilePath, destinationFilePath, xsltParameters));
        return result ?? string.Empty;
    }

    #endregion

    #region AI Operations

    private async Task<object> ExecuteAskGithubCopilotAsync(Dictionary<string, object> parameters)
    {
        var prompt = GetRequiredParameter<string>(parameters, "prompt");
        var promptName = GetOptionalParameter<string>(parameters, "promptName");
        return await _aiService.AskGithubCopilotAsync(prompt, promptName);
    }

    #endregion

    #region Parameter Helpers

    /// <summary>
    /// Gets a required parameter from the parameters dictionary
    /// </summary>
    private T GetRequiredParameter<T>(Dictionary<string, object> parameters, params string[] possibleKeys)
    {
        foreach (var key in possibleKeys)
        {
            if (parameters.TryGetValue(key, out var value))
            {
                if (value is T typedValue)
                {
                    return typedValue;
                }

                // Try to convert
                try
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                    throw new InvalidOperationException(
                        $"Parameter '{key}' has invalid type. Expected {typeof(T).Name}, got {value?.GetType().Name ?? "null"}");
                }
            }
        }

        throw new ArgumentException($"Required parameter not found. Tried keys: {string.Join(", ", possibleKeys)}");
    }

    /// <summary>
    /// Gets an optional parameter from the parameters dictionary
    /// </summary>
    private T? GetOptionalParameter<T>(Dictionary<string, object> parameters, string key, T? defaultValue = default)
    {
        if (parameters.TryGetValue(key, out var value))
        {
            if (value is T typedValue)
            {
                return typedValue;
            }

            // Try to convert
            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                _logger.LogWarning("Failed to convert parameter '{Key}' to type {Type}, using default", key, typeof(T).Name);
                return defaultValue;
            }
        }

        return defaultValue;
    }

    #endregion

    #endregion
}
