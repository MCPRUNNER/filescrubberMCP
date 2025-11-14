namespace filescrubberMCP.Interfaces;

/// <summary>
/// Service interface for parsing and searching operations on various file formats
/// </summary>
public interface IParserService
{
    /// <summary>
    /// Searches for values in a JSON file using JSONPath queries
    /// </summary>
    /// <param name="jsonFilePath">The path to the JSON file relative to workspace root</param>
    /// <param name="jsonPath">The JSONPath query string</param>
    /// <param name="indented">Whether to format the output as indented JSON</param>
    /// <param name="showKeyPaths">Whether to return structured results with path, value, and key information</param>
    /// <returns>A string representation of the search results, or null if an error occurs</returns>
    string? SearchJsonFile(string jsonFilePath, string jsonPath, bool indented = true, bool showKeyPaths = false);

    /// <summary>
    /// Searches for values in a CSV file using JSONPath queries
    /// </summary>
    /// <param name="csvFilePath">The path to the CSV file relative to workspace root</param>
    /// <param name="jsonPath">The JSONPath query string</param>
    /// <param name="hasHeaderRecord">Whether the CSV has a header record</param>
    /// <param name="ignoreBlankLines">Whether to ignore blank lines</param>
    /// <returns>A string representation of the search results, or null if an error occurs</returns>
    string? SearchCsvFile(string csvFilePath, string jsonPath, bool hasHeaderRecord = true, bool ignoreBlankLines = true);

    /// <summary>
    /// Searches for values in an XML file using XPath queries
    /// </summary>
    /// <param name="xmlFilePath">The path to the XML file relative to workspace root</param>
    /// <param name="xPath">The XPath query string</param>
    /// <param name="indented">Whether to format the output as indented XML</param>
    /// <param name="showKeyPaths">Whether to return structured results with path, value, and key information</param>
    /// <returns>A string representation of the search results, or null if an error occurs</returns>
    string? SearchXmlFile(string xmlFilePath, string xPath, bool indented = true, bool showKeyPaths = false);

    /// <summary>
    /// Searches for values in a YAML file using JSONPath queries
    /// </summary>
    /// <param name="yamlFilePath">The path to the YAML file relative to workspace root</param>
    /// <param name="jsonPath">The JSONPath query string</param>
    /// <param name="indented">Whether to format the output as indented JSON</param>
    /// <param name="showKeyPaths">Whether to return structured results with path, value, and key information</param>
    /// <returns>A string representation of the search results, or null if an error occurs</returns>
    string? SearchYamlFile(string yamlFilePath, string jsonPath, bool indented = true, bool showKeyPaths = false);

    /// <summary>
    /// Searches for values in an Excel file using JSONPath queries
    /// </summary>
    /// <param name="excelFilePath">The path to the Excel file relative to workspace root</param>
    /// <param name="jsonPath">The JSONPath query string</param>
    /// <returns>A string representation of the search results, or null if an error occurs</returns>
    string? SearchExcelFile(string excelFilePath, string jsonPath);

    /// <summary>
    /// Transforms an XML file using an XSLT stylesheet
    /// </summary>
    /// <param name="xmlFilePath">The path to the XML file to transform</param>
    /// <param name="xsltFilePath">The path to the XSLT stylesheet file</param>
    /// <param name="destinationFilePath">Optional path to save the transformed XML</param>
    /// <param name="xsltParameters">Optional dictionary of XSLT parameters to pass to the transformation</param>
    /// <returns>The transformed XML as a string, or null if an error occurs</returns>
    string? TransformXmlWithXslt(string xmlFilePath, string xsltFilePath, string? destinationFilePath = null, Dictionary<string, string>? xsltParameters = null);
}
