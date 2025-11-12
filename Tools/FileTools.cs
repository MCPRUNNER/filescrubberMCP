using System.ComponentModel;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using filescrubberMCP.Interfaces;
using filescrubberMCP.Models;

namespace filescrubberMCP.Tools;

/// <summary>
/// Provides MCP tools for file operations
/// </summary>
[McpServerToolType]
public class FileTools : IFileTools
{
    private readonly ILogger<FileTools> _logger;
    private readonly IFileService _fileService;

    /// <summary>
    /// Initializes a new instance of the FileTools class
    /// </summary>
    /// <param name="logger">Logger for the tools</param>
    /// <param name="fileService">Service for file operations</param>
    public FileTools(
        ILogger<FileTools> logger,
        IFileService fileService)
    {
        _logger = logger;
        _fileService = fileService;
    }

    /// <summary>
    /// Reads the content of a file
    /// </summary>
    [McpServerTool(Name = "fscrub_file_read"), Description("Reads the content of a file from the specified path.")]
    public async Task<string> ReadFile([Description("Path to the file to read from (relative to workspace root or absolute path)")] string filePath)
    {
        try
        {
            var content = await _fileService.ReadFileAsync(filePath);
            return JsonConvert.SerializeObject(new
            {
                success = true,
                filePath = filePath,
                content = content,
                contentLength = content.Length
            }, Formatting.Indented);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ReadFile tool for path: {FilePath}", filePath);
            return JsonConvert.SerializeObject(new
            {
                success = false,
                filePath = filePath,
                error = ex.Message
            }, Formatting.Indented);
        }
    }

    /// <summary>
    /// Writes content to a file
    /// </summary>
    [McpServerTool(Name = "fscrub_file_write"), Description("Writes text content to a file at the specified path. Creates the file and directory structure if they don't exist.")]
    public async Task<string> WriteFile([Description("Path to the file to write to (relative to workspace root or absolute path)")] string filePath, [Description("Content to write to the file")] string content)
    {
        try
        {
            await _fileService.WriteFileAsync(filePath, content);
            return JsonConvert.SerializeObject(new
            {
                success = true,
                filePath = filePath,
                contentLength = content.Length,
                message = "File written successfully"
            }, Formatting.Indented);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in WriteFile tool for path: {FilePath}", filePath);
            return JsonConvert.SerializeObject(new
            {
                success = false,
                filePath = filePath,
                error = ex.Message
            }, Formatting.Indented);
        }
    }

    /// <summary>
    /// Lists all files in a directory
    /// </summary>
    [McpServerTool(Name = "fscrub_file_list"), Description("Lists all files in a directory with metadata including full path, size, dates, and attributes. Supports search patterns and recursive search.")]
    public async Task<string> ListFiles(string directoryPath, string? searchPattern = null, bool recursive = true)
    {
        try
        {
            var pattern = string.IsNullOrWhiteSpace(searchPattern) ? "*" : searchPattern;
            var files = await _fileService.ListFilesAsync(directoryPath, pattern, recursive);

            return JsonConvert.SerializeObject(new
            {
                success = true,
                directoryPath = directoryPath,
                searchPattern = pattern,
                recursive = recursive,
                fileCount = files.Count,
                files = files
            }, Formatting.Indented);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ListFiles tool for directory: {DirectoryPath}", directoryPath);
            return JsonConvert.SerializeObject(new
            {
                success = false,
                directoryPath = directoryPath,
                error = ex.Message
            }, Formatting.Indented);
        }
    }
}
