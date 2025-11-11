namespace filescrubberMCP.Interfaces;

/// <summary>
/// Interface for file MCP tools functionality
/// </summary>
public interface IFileTools
{
    /// <summary>
    /// Reads the content of a file
    /// </summary>
    /// <param name="filePath">The path to the file to read</param>
    /// <returns>JSON representation of the result</returns>
    Task<string> ReadFile(string filePath);

    /// <summary>
    /// Writes content to a file
    /// </summary>
    /// <param name="filePath">The path to the file to write</param>
    /// <param name="content">The content to write</param>
    /// <returns>JSON representation of the result</returns>
    Task<string> WriteFile(string filePath, string content);

    /// <summary>
    /// Lists all files in a directory
    /// </summary>
    /// <param name="directoryPath">The directory path to search</param>
    /// <param name="searchPattern">Optional search pattern (default: "*")</param>
    /// <param name="recursive">Whether to search recursively (default: true)</param>
    /// <returns>JSON representation of file metadata list</returns>
    Task<string> ListFiles(string directoryPath, string? searchPattern = null, bool recursive = true);
}
