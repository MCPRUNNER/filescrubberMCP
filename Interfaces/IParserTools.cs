namespace filescrubberMCP.Interfaces;

/// <summary>
/// Interface for MCP tools that provide file parsing and search operations
/// </summary>
public interface IParserTools
{
    /// <summary>
    /// Searches for values in a JSON file using JSONPath queries
    /// </summary>
    Task<string> SearchJson(string jsonFilePath, string jsonPath, bool indented = true, bool showKeyPaths = false);

    /// <summary>
    /// Searches for values in a CSV file using JSONPath queries
    /// </summary>
    Task<string> SearchCsv(string csvFilePath, string jsonPath, bool hasHeaderRecord = true, bool ignoreBlankLines = true);

    /// <summary>
    /// Searches for values in an XML file using XPath queries
    /// </summary>
    Task<string> SearchXml(string xmlFilePath, string xPath, bool indented = true, bool showKeyPaths = false);

    /// <summary>
    /// Searches for values in a YAML file using JSONPath queries
    /// </summary>
    Task<string> SearchYaml(string yamlFilePath, string jsonPath, bool indented = true, bool showKeyPaths = false);

    /// <summary>
    /// Searches for values in an Excel file using JSONPath queries
    /// </summary>
    Task<string> SearchExcel(string excelFilePath, string jsonPath);

    /// <summary>
    /// Transforms an XML file using an XSLT stylesheet
    /// </summary>
    Task<string> TransformXml(string xmlFilePath, string xsltFilePath, string? destinationFilePath = null);
}
