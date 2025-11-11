namespace filescrubberMCP.Interfaces;

/// <summary>
/// Service interface for file operations
/// </summary>
public interface IFileService
{
  /// <summary>
  /// Reads the content of a file asynchronously
  /// </summary>
  /// <param name="filePath">The path to the file to read</param>
  /// <returns>The content of the file as a string</returns>
  Task<string> ReadFileAsync(string filePath);

  /// <summary>
  /// Writes text content to a file asynchronously
  /// </summary>
  /// <param name="filePath">The path to the file to write</param>
  /// <param name="content">The text content to write</param>
  /// <returns>A task representing the asynchronous operation</returns>
  Task WriteFileAsync(string filePath, string content);

  /// <summary>
  /// Lists all files under a provided path and returns metadata about each file
  /// </summary>
  /// <param name="directoryPath">The directory path to search</param>
  /// <param name="searchPattern">Optional search pattern (default: "*" for all files)</param>
  /// <param name="recursive">Whether to search recursively in subdirectories (default: true)</param>
  /// <returns>A list of file metadata</returns>
  Task<List<Models.FileMetadata>> ListFilesAsync(string directoryPath, string searchPattern = "*", bool recursive = true);
}
