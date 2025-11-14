using System.ComponentModel;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using filescrubberMCP.Interfaces;

namespace filescrubberMCP.Tools;

/// <summary>
/// Provides MCP tools for file parsing and search operations
/// </summary>
[McpServerToolType]
public class ParserTools : IParserTools
{
    private readonly ILogger<ParserTools> _logger;
    private readonly IParserService _parserService;

    /// <summary>
    /// Initializes a new instance of the ParserTools class
    /// </summary>
    /// <param name="logger">Logger for the tools</param>
    /// <param name="parserService">Service for parser operations</param>
    public ParserTools(
        ILogger<ParserTools> logger,
        IParserService parserService)
    {
        _logger = logger;
        _parserService = parserService;
    }

    /// <summary>
    /// Searches for values in a JSON file using JSONPath queries
    /// </summary>
    [McpServerTool(Name = "fscrub_parser_search_json"), Description("Searches for values in a JSON file using JSONPath queries and returns the matching results.")]
    public async Task<string> SearchJson(string jsonFilePath, string jsonPath, bool indented = true, bool showKeyPaths = false)
    {
        try
        {
            var result = await Task.Run(() => _parserService.SearchJsonFile(jsonFilePath, jsonPath, indented, showKeyPaths));

            if (result == null)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    filePath = jsonFilePath,
                    jsonPath = jsonPath,
                    error = "Failed to search JSON file"
                }, Formatting.Indented);
            }

            return JsonConvert.SerializeObject(new
            {
                success = true,
                filePath = jsonFilePath,
                jsonPath = jsonPath,
                results = result
            }, Formatting.Indented);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SearchJson tool for file: {FilePath}", jsonFilePath);
            return JsonConvert.SerializeObject(new
            {
                success = false,
                filePath = jsonFilePath,
                jsonPath = jsonPath,
                error = ex.Message
            }, Formatting.Indented);
        }
    }

    /// <summary>
    /// Searches for values in a CSV file using JSONPath queries
    /// </summary>
    [McpServerTool(Name = "fscrub_parser_search_csv"), Description("Searches for values in a CSV file using JSONPath queries. The CSV is converted to JSON for querying.")]
    public async Task<string> SearchCsv(string csvFilePath, string jsonPath, bool hasHeaderRecord = true, bool ignoreBlankLines = true)
    {
        try
        {
            var result = await Task.Run(() => _parserService.SearchCsvFile(csvFilePath, jsonPath, hasHeaderRecord, ignoreBlankLines));

            if (result == null)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    filePath = csvFilePath,
                    jsonPath = jsonPath,
                    error = "Failed to search CSV file"
                }, Formatting.Indented);
            }

            return JsonConvert.SerializeObject(new
            {
                success = true,
                filePath = csvFilePath,
                jsonPath = jsonPath,
                results = result
            }, Formatting.Indented);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SearchCsv tool for file: {FilePath}", csvFilePath);
            return JsonConvert.SerializeObject(new
            {
                success = false,
                filePath = csvFilePath,
                jsonPath = jsonPath,
                error = ex.Message
            }, Formatting.Indented);
        }
    }

    /// <summary>
    /// Searches for values in an XML file using XPath queries
    /// </summary>
    [McpServerTool(Name = "fscrub_parser_search_xml"), Description("Searches for values in an XML file using XPath queries and returns the matching XML nodes or values.")]
    public async Task<string> SearchXml(string xmlFilePath, string xPath, bool indented = true, bool showKeyPaths = false)
    {
        try
        {
            var result = await Task.Run(() => _parserService.SearchXmlFile(xmlFilePath, xPath, indented, showKeyPaths));

            if (result == null)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    filePath = xmlFilePath,
                    xPath = xPath,
                    error = "Failed to search XML file"
                }, Formatting.Indented);
            }

            return JsonConvert.SerializeObject(new
            {
                success = true,
                filePath = xmlFilePath,
                xPath = xPath,
                results = result
            }, Formatting.Indented);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SearchXml tool for file: {FilePath}", xmlFilePath);
            return JsonConvert.SerializeObject(new
            {
                success = false,
                filePath = xmlFilePath,
                xPath = xPath,
                error = ex.Message
            }, Formatting.Indented);
        }
    }

    /// <summary>
    /// Searches for values in a YAML file using JSONPath queries
    /// </summary>
    [McpServerTool(Name = "fscrub_parser_search_yaml"), Description("Searches for values in a YAML file using JSONPath queries. The YAML is converted to JSON for querying.")]
    public async Task<string> SearchYaml(string yamlFilePath, string jsonPath, bool indented = true, bool showKeyPaths = false)
    {
        try
        {
            var result = await Task.Run(() => _parserService.SearchYamlFile(yamlFilePath, jsonPath, indented, showKeyPaths));

            if (result == null)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    filePath = yamlFilePath,
                    jsonPath = jsonPath,
                    error = "Failed to search YAML file"
                }, Formatting.Indented);
            }

            return JsonConvert.SerializeObject(new
            {
                success = true,
                filePath = yamlFilePath,
                jsonPath = jsonPath,
                results = result
            }, Formatting.Indented);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SearchYaml tool for file: {FilePath}", yamlFilePath);
            return JsonConvert.SerializeObject(new
            {
                success = false,
                filePath = yamlFilePath,
                jsonPath = jsonPath,
                error = ex.Message
            }, Formatting.Indented);
        }
    }

    /// <summary>
    /// Searches for values in an Excel file using JSONPath queries
    /// </summary>
    [McpServerTool(Name = "fscrub_parser_search_excel"), Description("Searches for values in an Excel file using JSONPath queries. Each worksheet is converted to JSON for querying.")]
    public async Task<string> SearchExcel(string excelFilePath, string jsonPath)
    {
        try
        {
            var result = await Task.Run(() => _parserService.SearchExcelFile(excelFilePath, jsonPath));

            if (result == null)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    filePath = excelFilePath,
                    jsonPath = jsonPath,
                    error = "Failed to search Excel file"
                }, Formatting.Indented);
            }

            return JsonConvert.SerializeObject(new
            {
                success = true,
                filePath = excelFilePath,
                jsonPath = jsonPath,
                results = result
            }, Formatting.Indented);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SearchExcel tool for file: {FilePath}", excelFilePath);
            return JsonConvert.SerializeObject(new
            {
                success = false,
                filePath = excelFilePath,
                jsonPath = jsonPath,
                error = ex.Message
            }, Formatting.Indented);
        }
    }

    /// <summary>
    /// Transforms an XML file using an XSLT stylesheet
    /// </summary>
    [McpServerTool(Name = "fscrub_parser_transform_xml"), Description("Transforms an XML file using an XSLT stylesheet and returns the transformed XML. Optionally saves to a destination file and accepts XSLT parameters.")]
    public async Task<string> TransformXml(string xmlFilePath, string xsltFilePath, string? destinationFilePath = null, Dictionary<string, string>? xsltParameters = null)
    {
        try
        {
            var result = await Task.Run(() => _parserService.TransformXmlWithXslt(xmlFilePath, xsltFilePath, destinationFilePath, xsltParameters));

            if (result == null)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    xmlFilePath = xmlFilePath,
                    xsltFilePath = xsltFilePath,
                    destinationFilePath = destinationFilePath,
                    error = "Failed to transform XML file"
                }, Formatting.Indented);
            }

            return JsonConvert.SerializeObject(new
            {
                success = true,
                xmlFilePath = xmlFilePath,
                xsltFilePath = xsltFilePath,
                destinationFilePath = destinationFilePath,
                transformedXml = result
            }, Formatting.Indented);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in TransformXml tool for file: {FilePath}", xmlFilePath);
            return JsonConvert.SerializeObject(new
            {
                success = false,
                xmlFilePath = xmlFilePath,
                xsltFilePath = xsltFilePath,
                destinationFilePath = destinationFilePath,
                error = ex.Message
            }, Formatting.Indented);
        }
    }
}
