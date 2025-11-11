namespace filescrubberMCP.Interfaces;

/// <summary>
/// Interface for Scriban template processing service
/// </summary>
public interface ITemplateService
{
    /// <summary>
    /// Processes a Scriban template file with JSON data and saves the result to an output file
    /// </summary>
    /// <param name="templateFilePath">Path to the .sbn template file</param>
    /// <param name="jsonData">JSON data as string or object to be passed to the template</param>
    /// <param name="outputFilePath">Path where the processed template output will be saved</param>
    /// <returns>A task representing the asynchronous operation with the output file path on success, or error message on failure</returns>
    Task<string> ProcessTemplateAsync(string templateFilePath, object jsonData, string outputFilePath);

    /// <summary>
    /// Processes a Scriban template file with JSON data and returns the result as a string
    /// </summary>
    /// <param name="templateFilePath">Path to the .sbn template file</param>
    /// <param name="jsonData">JSON data as string or object to be passed to the template</param>
    /// <returns>A task representing the asynchronous operation with the rendered output as a string</returns>
    Task<string> RenderTemplateAsync(string templateFilePath, object jsonData);
}
