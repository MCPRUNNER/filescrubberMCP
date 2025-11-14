using Microsoft.Extensions.Logging;
using filescrubberMCP.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using ClosedXML.Excel;
using YamlDotNet.Serialization;

namespace filescrubberMCP.Services;

/// <summary>
/// Implementation of parser service that provides search and transformation capabilities for various file formats
/// </summary>
public class ParserService : IParserService
{
    private readonly ILogger<ParserService> _logger;
    private readonly IFileService _fileService;
    private readonly string _workspaceRoot;

    /// <summary>
    /// Initializes a new instance of the ParserService class
    /// </summary>
    /// <param name="logger">Logger for the service</param>
    /// <param name="fileService">File service for file operations</param>
    public ParserService(
        ILogger<ParserService> logger,
        IFileService fileService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _workspaceRoot = Environment.CurrentDirectory; // Default to current directory
    }

    #region JSON Operations

    /// <summary>
    /// Searches for values in a JSON file using JSONPath queries
    /// </summary>
    public string? SearchJsonFile(string jsonFilePath, string jsonPath, bool indented = true, bool showKeyPaths = false)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(jsonFilePath))
            {
                _logger.LogError("JSON file path cannot be null or empty");
                return null;
            }

            if (string.IsNullOrWhiteSpace(jsonPath))
            {
                _logger.LogError("JSONPath cannot be null or empty");
                return null;
            }

            var filePath = GetFullPath(jsonFilePath);
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("JSON file does not exist: {FilePath}", filePath);
                return null;
            }

            var jsonContent = File.ReadAllText(filePath);
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                _logger.LogError("JSON file content is empty or null: {JsonFilePath}", jsonFilePath);
                return null;
            }

            var results = ExtractJTokens(jsonContent, jsonPath);
            if (results == null || !results.Any())
            {
                _logger.LogWarning("No matches found for JSONPath: {JsonPath} in file: {JsonFilePath}", jsonPath, jsonFilePath);
                return string.Empty;
            }

            var resultsList = results.ToList();
            if (showKeyPaths)
            {
                return FormatJsonResultsWithPaths(resultsList, indented);
            }
            else
            {
                return FormatJsonResults(resultsList, indented);
            }
        }
        catch (FileNotFoundException)
        {
            _logger.LogError("The file '{JsonFilePath}' was not found", jsonFilePath);
            return null;
        }
        catch (JsonReaderException ex)
        {
            _logger.LogError(ex, "Invalid JSON format in '{JsonFilePath}'. Details: {Message}", jsonFilePath, ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while searching JSON file '{JsonFilePath}': {Message}", jsonFilePath, ex.Message);
            return null;
        }
    }

    #endregion

    #region CSV Operations

    /// <summary>
    /// Searches for values in a CSV file using JSONPath queries
    /// </summary>
    public string? SearchCsvFile(string csvFilePath, string jsonPath, bool hasHeaderRecord = true, bool ignoreBlankLines = true)
    {
        if (!ValidateInputs(csvFilePath, jsonPath))
            return null;

        var fullCsvPath = GetFullPath(csvFilePath);
        if (!File.Exists(fullCsvPath))
        {
            _logger.LogWarning("CSV file does not exist: {CsvFilePath}", csvFilePath);
            return null;
        }

        try
        {
            var records = ReadCsvRecords(fullCsvPath, hasHeaderRecord, ignoreBlankLines);
            if (records == null || !records.Any())
            {
                _logger.LogWarning("No records found in CSV file: {CsvFilePath}", csvFilePath);
                return null;
            }

            var tokens = ExtractJTokens(records, jsonPath);
            if (tokens == null || !tokens.Any())
            {
                _logger.LogWarning("No matches found for JSONPath: {JsonPath} in CSV file: {CsvFilePath}", jsonPath, csvFilePath);
                return string.Empty;
            }

            var results = tokens.ToList();
            if (!results.Any())
            {
                _logger.LogWarning("No matches found for JSONPath: {JsonPath} in CSV file: {CsvFilePath}", jsonPath, csvFilePath);
                return string.Empty;
            }

            return results.Count == 1
                ? results[0].ToString(Newtonsoft.Json.Formatting.Indented)
                : new JArray(results).ToString(Newtonsoft.Json.Formatting.Indented);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching CSV file: {CsvFilePath}", csvFilePath);
            return null;
        }
    }

    #endregion

    #region XML Operations

    /// <summary>
    /// Searches for values in an XML file using XPath queries
    /// </summary>
    public string? SearchXmlFile(string xmlFilePath, string xPath, bool indented = true, bool showKeyPaths = false)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(xmlFilePath))
            {
                _logger.LogError("XML file path cannot be null or empty");
                return null;
            }

            if (string.IsNullOrWhiteSpace(xPath))
            {
                _logger.LogError("XPath cannot be null or empty");
                return null;
            }

            var filePath = Path.Combine(_workspaceRoot, xmlFilePath);
            var xmlContent = File.ReadAllText(filePath);
            if (string.IsNullOrWhiteSpace(xmlContent))
            {
                _logger.LogError("XML file content is empty or null: {XmlFilePath}", xmlFilePath);
                return null;
            }

            XDocument xmlDoc = XDocument.Parse(xmlContent);
            var xPathResults = xmlDoc.XPathEvaluate(xPath);
            var resultsList = new List<object>();

            if (xPathResults is IEnumerable<object> enumerable)
            {
                resultsList.AddRange(enumerable);
            }
            else if (xPathResults != null)
            {
                resultsList.Add(xPathResults);
            }

            if (!resultsList.Any())
            {
                _logger.LogWarning("No matches found for XPath: {XPath} in file: {XmlFilePath}", xPath, xmlFilePath);
                return string.Empty;
            }

            _logger.LogInformation("Successfully found {Count} matches for XPath: {XPath} in file: {XmlFilePath}",
                resultsList.Count, xPath, xmlFilePath);

            return FormatXmlResults(resultsList, indented, showKeyPaths, xPath);
        }
        catch (FileNotFoundException)
        {
            _logger.LogError("The file '{XmlFilePath}' was not found", xmlFilePath);
            return null;
        }
        catch (XmlException ex)
        {
            _logger.LogError(ex, "Invalid XML format in '{XmlFilePath}'. Details: {Message}", xmlFilePath, ex.Message);
            return null;
        }
        catch (XPathException ex)
        {
            _logger.LogError(ex, "Invalid XPath expression '{XPath}'. Details: {Message}", xPath, ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while searching XML file '{XmlFilePath}': {Message}", xmlFilePath, ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Transforms an XML file using an XSLT stylesheet
    /// </summary>
    public string? TransformXmlWithXslt(string xmlFilePath, string xsltFilePath, string? destinationFilePath = null, Dictionary<string, string>? xsltParameters = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(xmlFilePath))
            {
                _logger.LogError("XML file path cannot be null or empty");
                return null;
            }

            if (string.IsNullOrWhiteSpace(xsltFilePath))
            {
                _logger.LogError("XSLT file path cannot be null or empty");
                return null;
            }

            var fullXmlPath = GetFullPath(xmlFilePath);
            var fullXsltPath = GetFullPath(xsltFilePath);

            if (!File.Exists(fullXmlPath))
            {
                _logger.LogError("XML file not found: {XmlFilePath}", xmlFilePath);
                return null;
            }

            if (!File.Exists(fullXsltPath))
            {
                _logger.LogError("XSLT file not found: {XsltFilePath}", xsltFilePath);
                return null;
            }

            var xslt = new XslCompiledTransform();
            xslt.Load(fullXsltPath);

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(fullXmlPath);

            // Create XSLT argument list and add parameters if provided
            var xsltArgs = new XsltArgumentList();
            if (xsltParameters != null && xsltParameters.Count > 0)
            {
                _logger.LogInformation("Adding {Count} XSLT parameters", xsltParameters.Count);
                foreach (var param in xsltParameters)
                {
                    xsltArgs.AddParam(param.Key, string.Empty, param.Value);
                    _logger.LogInformation("Added XSLT parameter: {Key} = '{Value}'", param.Key, param.Value);
                }
            }
            else
            {
                _logger.LogInformation("No XSLT parameters provided");
            }

            using var stringWriter = new StringWriter();
            using var xmlWriter = XmlWriter.Create(stringWriter, xslt.OutputSettings);
            xslt.Transform(xmlDoc, xsltArgs, xmlWriter);
            var result = stringWriter.ToString();

            if (!string.IsNullOrWhiteSpace(destinationFilePath))
            {
                try
                {
                    var fullDestinationPath = Path.IsPathRooted(destinationFilePath)
                        ? destinationFilePath
                        : Path.Combine(_workspaceRoot, destinationFilePath);

                    var directory = Path.GetDirectoryName(fullDestinationPath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    File.WriteAllText(fullDestinationPath, result);
                    _logger.LogInformation("Successfully saved transformed XML to: {DestinationFilePath}", destinationFilePath);
                }
                catch (Exception saveEx)
                {
                    _logger.LogError(saveEx, "Error saving transformed XML to destination file: {DestinationFilePath}", destinationFilePath);
                }
            }

            _logger.LogInformation("Successfully transformed XML file '{XmlFilePath}' using XSLT '{XsltFilePath}'",
                xmlFilePath, xsltFilePath);
            return result;
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex, "File not found during XSLT transformation: {Message}", ex.Message);
            return null;
        }
        catch (XmlException ex)
        {
            _logger.LogError(ex, "Invalid XML format during XSLT transformation. Details: {Message}", ex.Message);
            return null;
        }
        catch (XsltException ex)
        {
            _logger.LogError(ex, "XSLT transformation error. Details: {Message}", ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during XSLT transformation: {Message}", ex.Message);
            return null;
        }
    }

    #endregion

    #region YAML Operations

    /// <summary>
    /// Searches for values in a YAML file using JSONPath queries
    /// </summary>
    public string? SearchYamlFile(string yamlFilePath, string jsonPath, bool indented = true, bool showKeyPaths = false)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(yamlFilePath))
            {
                _logger.LogError("YAML file path cannot be null or empty");
                return null;
            }

            if (string.IsNullOrWhiteSpace(jsonPath))
            {
                _logger.LogError("JSONPath cannot be null or empty");
                return null;
            }

            var filePath = GetFullPath(yamlFilePath);
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("YAML file does not exist: {YamlFilePath}", yamlFilePath);
                return null;
            }

            var yamlContent = File.ReadAllText(filePath);
            if (string.IsNullOrWhiteSpace(yamlContent))
            {
                _logger.LogError("YAML file content is empty or null: {YamlFilePath}", yamlFilePath);
                return null;
            }

            // Parse YAML and convert to JSON
            var deserializer = new DeserializerBuilder().Build();
            var yamlObject = deserializer.Deserialize(yamlContent);
            var serializer = new SerializerBuilder()
                .JsonCompatible()
                .Build();
            var jsonContent = serializer.Serialize(yamlObject);

            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                _logger.LogError("Failed to convert YAML content to JSON: {YamlFilePath}", yamlFilePath);
                return null;
            }

            var results = ExtractJTokens(jsonContent, jsonPath);
            if (results == null || !results.Any())
            {
                _logger.LogWarning("No matches found for JSONPath: {JsonPath} in YAML file: {YamlFilePath}", jsonPath, yamlFilePath);
                return string.Empty;
            }

            var resultsList = results.ToList();
            if (showKeyPaths)
            {
                return FormatJsonResultsWithPaths(resultsList, indented);
            }
            else
            {
                return FormatJsonResults(resultsList, indented);
            }
        }
        catch (FileNotFoundException)
        {
            _logger.LogError("The YAML file '{YamlFilePath}' was not found", yamlFilePath);
            return null;
        }
        catch (YamlDotNet.Core.YamlException ex)
        {
            _logger.LogError(ex, "Invalid YAML format in '{YamlFilePath}'. Details: {Message}", yamlFilePath, ex.Message);
            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error processing YAML conversion to JSON in '{YamlFilePath}'. Details: {Message}", yamlFilePath, ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while searching YAML file '{YamlFilePath}': {Message}", yamlFilePath, ex.Message);
            return null;
        }
    }

    #endregion

    #region Excel Operations

    /// <summary>
    /// Searches for values in an Excel file using JSONPath queries
    /// </summary>
    public string? SearchExcelFile(string excelFilePath, string jsonPath)
    {
        if (!ValidateInputs(excelFilePath, jsonPath))
            return null;

        var fullExcelPath = GetFullPath(excelFilePath);
        if (!File.Exists(fullExcelPath))
        {
            _logger.LogWarning("Excel file does not exist: {ExcelFilePath}", excelFilePath);
            return null;
        }

        try
        {
            using var workbook = new XLWorkbook(fullExcelPath);
            var workbookJson = new JObject();

            foreach (var worksheet in workbook.Worksheets)
            {
                var rows = new List<Dictionary<string, object>>();
                var firstRow = worksheet.FirstRowUsed();
                if (firstRow == null) continue;

                var headerRow = firstRow.RowUsed();
                var headers = headerRow.Cells().Select(c => c.GetString()).ToList();
                var headerCells = headerRow.Cells().ToList();
                var headerLetters = headerCells.Select(c => c.Address.ColumnLetter).ToList();

                foreach (var dataRow in worksheet.RowsUsed().Skip(1))
                {
                    var rowDict = new Dictionary<string, object>();
                    var cells = dataRow.Cells().ToList();

                    for (var i = 0; i < headers.Count && i < cells.Count; i++)
                    {
                        var value = cells[i].GetString();
                        rowDict[headers[i]] = value;

                        if (!headers.Contains(headerLetters[i]))
                        {
                            rowDict[headerLetters[i]] = value;
                        }
                    }

                    rows.Add(rowDict);
                }

                workbookJson[worksheet.Name] = JArray.FromObject(rows);
            }

            var fullJson = workbookJson.ToString(Newtonsoft.Json.Formatting.None);
            var matchedTokens = ExtractJTokens(fullJson, jsonPath);
            var resultToken = (matchedTokens != null && matchedTokens.Any())
                ? (matchedTokens.Count() == 1 ? matchedTokens.First() : new JArray(matchedTokens))
                : new JArray();

            return resultToken.ToString(Newtonsoft.Json.Formatting.Indented);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching Excel file: {ExcelFilePath}", excelFilePath);
            return null;
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Gets the full path for a file relative to workspace root
    /// </summary>
    private string GetFullPath(string filePath)
    {
        if (Path.IsPathRooted(filePath))
            return filePath;

        return Path.Combine(_workspaceRoot, filePath);
    }

    /// <summary>
    /// Validates input parameters
    /// </summary>
    private bool ValidateInputs(string filePath, string queryPath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            _logger.LogError("File path cannot be null or empty");
            return false;
        }

        if (string.IsNullOrWhiteSpace(queryPath))
        {
            _logger.LogError("Query path cannot be null or empty");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Reads CSV records from a file
    /// </summary>
    private List<dynamic> ReadCsvRecords(string fullCsvPath, bool hasHeaderRecord, bool ignoreBlankLines)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = hasHeaderRecord,
            IgnoreBlankLines = ignoreBlankLines,
            TrimOptions = TrimOptions.Trim
        };

        using var reader = new StreamReader(fullCsvPath);
        using var csv = new CsvReader(reader, config);
        return csv.GetRecords<dynamic>().ToList();
    }

    /// <summary>
    /// Extracts JSON tokens from JSON content using JSONPath
    /// </summary>
    private IEnumerable<JToken> ExtractJTokens(string jsonContent, string jsonPath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(jsonPath))
            {
                _logger.LogError("JSON Path cannot be null or empty");
                return Enumerable.Empty<JToken>();
            }

            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                _logger.LogError("JSON content is empty or null");
                return new List<JToken>();
            }

            JToken jsonToken = JToken.Parse(jsonContent);
            return jsonToken.SelectTokens(jsonPath).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting JSON tokens");
            return new List<JToken>();
        }
    }

    /// <summary>
    /// Extracts JSON tokens from a list of records
    /// </summary>
    private IEnumerable<JToken> ExtractJTokens(List<object> records, string jsonPath)
    {
        var jsonContent = JsonConvert.SerializeObject(records, Newtonsoft.Json.Formatting.None);
        if (string.IsNullOrWhiteSpace(jsonContent))
        {
            _logger.LogError("JSON content is empty or null");
            return new List<JToken>();
        }

        var jsonArray = JArray.Parse(jsonContent);
        if (string.IsNullOrWhiteSpace(jsonPath))
        {
            _logger.LogError("JSON Path cannot be null or empty");
            return new List<JToken>();
        }

        var results = jsonArray.SelectTokens(jsonPath).ToList();
        if (results == null || !results.Any())
        {
            _logger.LogWarning("No matches found for JSONPath: {JsonPath}", jsonPath);
            return new List<JToken>();
        }

        return results;
    }

    /// <summary>
    /// Formats JSON results
    /// </summary>
    private string FormatJsonResults(List<JToken> results, bool indented)
    {
        if (results.Count == 1)
        {
            return results[0].ToString(indented ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None);
        }
        else
        {
            var resultArray = new JArray(results);
            return resultArray.ToString(indented ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None);
        }
    }

    /// <summary>
    /// Formats JSON results with path information
    /// </summary>
    private string FormatJsonResultsWithPaths(List<JToken> results, bool indented)
    {
        var structuredResults = new JArray();
        foreach (var result in results)
        {
            var pathInfo = new JObject
            {
                ["path"] = result.Path,
                ["value"] = result
            };

            var pathParts = result.Path.Split('.');
            if (pathParts.Length > 0)
            {
                var lastPart = pathParts.Last().Replace("[", "").Replace("]", "");
                if (!string.IsNullOrEmpty(lastPart) && !char.IsDigit(lastPart[0]))
                {
                    pathInfo["key"] = lastPart;
                }
            }

            structuredResults.Add(pathInfo);
        }

        if (structuredResults.Count == 1)
        {
            return structuredResults[0].ToString(indented ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None);
        }
        else
        {
            return structuredResults.ToString(indented ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None);
        }
    }

    /// <summary>
    /// Formats XML search results
    /// </summary>
    private string FormatXmlResults(List<object> results, bool indented, bool showKeyPaths, string xPath)
    {
        if (showKeyPaths)
        {
            var structuredResults = new JArray();
            for (var i = 0; i < results.Count; i++)
            {
                var result = results[i];
                var pathInfo = new JObject();
                string path;
                string value;
                string key;

                if (result is XElement element)
                {
                    path = GetXElementPath(element);
                    value = element.ToString(indented ? System.Xml.Linq.SaveOptions.None : System.Xml.Linq.SaveOptions.DisableFormatting);
                    key = element.Name.LocalName;
                }
                else if (result is XAttribute attribute)
                {
                    path = GetXAttributePath(attribute);
                    value = attribute.Value;
                    key = attribute.Name.LocalName;
                }
                else if (result is XText text)
                {
                    path = GetXTextPath(text);
                    value = text.Value;
                    key = "text";
                }
                else
                {
                    path = $"{xPath}[{i}]";
                    value = result.ToString() ?? "";
                    key = ExtractKeyFromXPath(xPath);
                }

                pathInfo["path"] = path;
                pathInfo["value"] = value;
                pathInfo["key"] = key;
                structuredResults.Add(pathInfo);
            }

            if (structuredResults.Count == 1)
            {
                return structuredResults[0].ToString(indented ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None);
            }
            else
            {
                return structuredResults.ToString(indented ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None);
            }
        }
        else
        {
            if (results.Count == 1)
            {
                var result = results[0];
                if (result is XElement element)
                {
                    return element.ToString(indented ? System.Xml.Linq.SaveOptions.None : System.Xml.Linq.SaveOptions.DisableFormatting);
                }
                else if (result is XAttribute attribute)
                {
                    return attribute.Value;
                }
                else
                {
                    return result.ToString() ?? "";
                }
            }
            else
            {
                var resultArray = new JArray();
                foreach (var result in results)
                {
                    if (result is XElement element)
                    {
                        resultArray.Add(element.ToString(System.Xml.Linq.SaveOptions.DisableFormatting));
                    }
                    else if (result is XAttribute attribute)
                    {
                        resultArray.Add(attribute.Value);
                    }
                    else
                    {
                        resultArray.Add(result.ToString());
                    }
                }

                return resultArray.ToString(indented ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None);
            }
        }
    }

    /// <summary>
    /// Gets the XPath for an XElement
    /// </summary>
    private string GetXElementPath(XElement element)
    {
        var path = new List<string>();
        var current = element;

        while (current != null)
        {
            var index = current.ElementsBeforeSelf(current.Name).Count();
            var name = current.Name.LocalName;

            if (index > 0)
            {
                path.Insert(0, $"{name}[{index + 1}]");
            }
            else
            {
                path.Insert(0, name);
            }

            current = current.Parent;
        }

        return "/" + string.Join("/", path);
    }

    /// <summary>
    /// Gets the XPath for an XAttribute
    /// </summary>
    private string GetXAttributePath(XAttribute attribute)
    {
        var elementPath = GetXElementPath(attribute.Parent ?? throw new InvalidOperationException("Attribute has no parent element"));
        return $"{elementPath}/@{attribute.Name.LocalName}";
    }

    /// <summary>
    /// Gets the XPath for an XText node
    /// </summary>
    private string GetXTextPath(XText text)
    {
        var elementPath = GetXElementPath(text.Parent ?? throw new InvalidOperationException("Text node has no parent element"));
        return $"{elementPath}/text()";
    }

    /// <summary>
    /// Extracts a meaningful key name from an XPath expression
    /// </summary>
    private string ExtractKeyFromXPath(string xPath)
    {
        var parts = xPath.TrimStart('/').Split('/');
        var lastPart = parts.LastOrDefault()?.Split('@').LastOrDefault()?.Split('[').FirstOrDefault();
        return !string.IsNullOrEmpty(lastPart) ? lastPart : "result";
    }

    #endregion
}
