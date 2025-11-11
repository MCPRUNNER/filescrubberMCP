namespace filescrubberMCP.Interfaces;

/// <summary>
/// Interface for Scriban template MCP tools functionality
/// </summary>
public interface ITemplateTools
{
    /// <summary>
    /// Processes a Scriban template file with JSON data and saves the result to an output file
    /// </summary>
    /// <param name="templateFilePath">Path to the .sbn template file</param>
    /// <param name="jsonData">JSON data as string to be passed to the template</param>
    /// <param name="outputFilePath">Path where the processed template output will be saved</param>
    /// <returns>JSON representation of the result containing the output file path or error message</returns>
    Task<string> ProcessTemplate(string templateFilePath, string jsonData, string outputFilePath);

    /// <summary>
    /// Processes a Scriban template file with JSON data and returns the rendered result
    /// </summary>
    /// <param name="templateFilePath">Path to the .sbn template file</param>
    /// <param name="jsonData">JSON data as string to be passed to the template</param>
    /// <returns>JSON representation of the result containing the rendered output or error message</returns>
    Task<string> RenderTemplate(string templateFilePath, string jsonData);
}
