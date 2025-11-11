using Microsoft.Extensions.Logging;
using filescrubberMCP.Interfaces;
using filescrubberMCP.Models;

namespace filescrubberMCP.Services;

/// <summary>
/// Service implementation for file operations
/// </summary>
public class FileService : IFileService
{
    private readonly ILogger<FileService> _logger;
    private readonly IAppConfigurationProvider _configurationProvider;

    /// <summary>
    /// Initializes a new instance of the FileService class
    /// </summary>
    /// <param name="logger">Logger for the service</param>
    /// <param name="configurationProvider">Provider for configuration</param>
    public FileService(
        ILogger<FileService> logger,
        IAppConfigurationProvider configurationProvider)
    {
        _logger = logger;
        _configurationProvider = configurationProvider;
    }

    /// <summary>
    /// Reads the content of a file asynchronously
    /// </summary>
    /// <param name="filePath">The path to the file to read</param>
    /// <returns>The content of the file as a string</returns>
    public async Task<string> ReadFileAsync(string filePath)
    {
        try
        {
            var normalizedPath = NormalizePath(filePath);
            _logger.LogInformation("Reading file from path: {FilePath}", normalizedPath);

            if (string.IsNullOrWhiteSpace(normalizedPath))
            {
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
            }

            if (!File.Exists(normalizedPath))
            {
                throw new FileNotFoundException($"File not found: {normalizedPath}", normalizedPath);
            }

            var content = await File.ReadAllTextAsync(normalizedPath);
            _logger.LogInformation("Successfully read {Length} characters from file: {FilePath}", content.Length, normalizedPath);

            return content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading file from path: {FilePath}", filePath);
            throw;
        }
    }

    /// <summary>
    /// Writes text content to a file asynchronously
    /// </summary>
    /// <param name="filePath">The path to the file to write</param>
    /// <param name="content">The text content to write</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task WriteFileAsync(string filePath, string content)
    {
        try
        {
            var normalizedPath = NormalizePath(filePath);
            _logger.LogInformation("Writing file to path: {FilePath}", normalizedPath);

            if (string.IsNullOrWhiteSpace(normalizedPath))
            {
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
            }

            if (content == null)
            {
                throw new ArgumentNullException(nameof(content), "Content cannot be null");
            }

            // Ensure directory exists
            var directory = Path.GetDirectoryName(normalizedPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                _logger.LogInformation("Created directory: {Directory}", directory);
            }

            await File.WriteAllTextAsync(normalizedPath, content);
            _logger.LogInformation("Successfully wrote {Length} characters to file: {FilePath}", content.Length, normalizedPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error writing file to path: {FilePath}", filePath);
            throw;
        }
    }

    /// <summary>
    /// Lists all files under a provided path and returns metadata about each file
    /// </summary>
    /// <param name="directoryPath">The directory path to search</param>
    /// <param name="searchPattern">Optional search pattern (default: "*" for all files)</param>
    /// <param name="recursive">Whether to search recursively in subdirectories (default: true)</param>
    /// <returns>A list of file metadata</returns>
    public async Task<List<FileMetadata>> ListFilesAsync(string directoryPath, string searchPattern = "*", bool recursive = true)
    {
        return await Task.Run(() =>
        {
            try
            {
                var normalizedPath = NormalizePath(directoryPath);
                _logger.LogInformation("Listing files in directory: {DirectoryPath}, Pattern: {SearchPattern}, Recursive: {Recursive}",
                    normalizedPath, searchPattern, recursive);

                if (string.IsNullOrWhiteSpace(normalizedPath))
                {
                    throw new ArgumentException("Directory path cannot be null or empty", nameof(directoryPath));
                }

                if (!Directory.Exists(normalizedPath))
                {
                    throw new DirectoryNotFoundException($"Directory not found: {normalizedPath}");
                }

                var fileMetadataList = new List<FileMetadata>();
                var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

                var files = Directory.GetFiles(normalizedPath, searchPattern, searchOption);

                foreach (var filePath in files)
                {
                    try
                    {
                        var fileInfo = new FileInfo(filePath);
                        var metadata = new FileMetadata
                        {
                            full_path = fileInfo.FullName,
                            file_name = fileInfo.Name,
                            file_name_without_extension = Path.GetFileNameWithoutExtension(fileInfo.Name),
                            extension = fileInfo.Extension,
                            directory_path = fileInfo.DirectoryName ?? string.Empty,
                            size_in_bytes = fileInfo.Length,
                            creation_time_utc = fileInfo.CreationTimeUtc,
                            last_write_time_utc = fileInfo.LastWriteTimeUtc,
                            last_access_time_utc = fileInfo.LastAccessTimeUtc,
                            attributes = fileInfo.Attributes,
                            is_read_only = fileInfo.IsReadOnly,
                            is_hidden = (fileInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden
                        };

                        fileMetadataList.Add(metadata);
                    }
                    catch (Exception fileEx)
                    {
                        _logger.LogWarning(fileEx, "Error reading metadata for file: {FilePath}", filePath);
                        // Continue processing other files
                    }
                }

                _logger.LogInformation("Successfully listed {Count} files from directory: {DirectoryPath}",
                    fileMetadataList.Count, normalizedPath);

                return fileMetadataList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing files in directory: {DirectoryPath}", directoryPath);
                throw;
            }
        });
    }

    /// <summary>
    /// Normalizes a file path to use the correct directory separator for the current platform
    /// and resolves it relative to the configured root directory if it's a relative path
    /// </summary>
    /// <param name="path">The path to normalize</param>
    /// <returns>The normalized path</returns>
    private string NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return path;
        }

        // Replace forward slashes with backslashes on Windows, and vice versa on Unix
        // Path.DirectorySeparatorChar automatically handles platform differences
        var normalized = path.Replace('\\', Path.DirectorySeparatorChar)
                             .Replace('/', Path.DirectorySeparatorChar);

        // Normalize consecutive separators to single separator
        while (normalized.Contains($"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}"))
        {
            normalized = normalized.Replace(
                $"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}",
                Path.DirectorySeparatorChar.ToString());
        }

        // If the path is relative, resolve it relative to the root directory
        if (!Path.IsPathRooted(normalized))
        {
            var rootDirectory = _configurationProvider.GetRootDirectory();
            normalized = Path.Combine(rootDirectory, normalized);
            _logger.LogDebug("Resolved relative path to: {NormalizedPath}", normalized);
        }

        return normalized;
    }
}
