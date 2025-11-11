using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using filescrubberMCP.Tools;
using filescrubberMCP.Interfaces;
using filescrubberMCP.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace filescrubberMCP.Tests.Tools;

public class FileToolsTests
{
    private readonly Mock<ILogger<FileTools>> _mockLogger;
    private readonly Mock<IFileService> _mockFileService;
    private readonly FileTools _fileTools;

    public FileToolsTests()
    {
        _mockLogger = new Mock<ILogger<FileTools>>();
        _mockFileService = new Mock<IFileService>();
        _fileTools = new FileTools(_mockLogger.Object, _mockFileService.Object);
    }

    [Fact]
    public async Task ReadFile_WithValidFile_ReturnsSuccess()
    {
        // Arrange
        var filePath = "test.txt";
        var content = "Test content";
        _mockFileService.Setup(x => x.ReadFileAsync(filePath))
            .ReturnsAsync(content);

        // Act
        var result = await _fileTools.ReadFile(filePath);

        // Assert
        Assert.Contains("\"success\": true", result);
        Assert.Contains(content, result);
        Assert.Contains(filePath, result);
        _mockFileService.Verify(x => x.ReadFileAsync(filePath), Times.Once);
    }

    [Fact]
    public async Task ReadFile_WhenServiceThrowsException_ReturnsError()
    {
        // Arrange
        var filePath = "nonexistent.txt";
        _mockFileService.Setup(x => x.ReadFileAsync(filePath))
            .ThrowsAsync(new System.IO.FileNotFoundException());

        // Act
        var result = await _fileTools.ReadFile(filePath);

        // Assert
        Assert.Contains("\"success\": false", result);
        Assert.Contains("error", result);
    }

    [Fact]
    public async Task WriteFile_WithValidInput_ReturnsSuccess()
    {
        // Arrange
        var filePath = "output.txt";
        var content = "Content to write";
        _mockFileService.Setup(x => x.WriteFileAsync(filePath, content))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _fileTools.WriteFile(filePath, content);

        // Assert
        Assert.Contains("\"success\": true", result);
        Assert.Contains(filePath, result);
        Assert.Contains("contentLength", result);
        _mockFileService.Verify(x => x.WriteFileAsync(filePath, content), Times.Once);
    }

    [Fact]
    public async Task WriteFile_WhenServiceThrowsException_ReturnsError()
    {
        // Arrange
        var filePath = "protected.txt";
        var content = "Content";
        _mockFileService.Setup(x => x.WriteFileAsync(filePath, content))
            .ThrowsAsync(new System.UnauthorizedAccessException());

        // Act
        var result = await _fileTools.WriteFile(filePath, content);

        // Assert
        Assert.Contains("\"success\": false", result);
        Assert.Contains("error", result);
    }

    [Fact]
    public async Task ListFiles_WithFiles_ReturnsSuccess()
    {
        // Arrange
        var directory = "c:\\test";
        var files = new List<FileMetadata>
        {
            new FileMetadata
            {
                file_name = "file1.txt",
                full_path = "c:\\test\\file1.txt",
                size_in_bytes = 100,
                extension = ".txt"
            },
            new FileMetadata
            {
                file_name = "file2.txt",
                full_path = "c:\\test\\file2.txt",
                size_in_bytes = 200,
                extension = ".txt"
            }
        };
        _mockFileService.Setup(x => x.ListFilesAsync(directory, "*", true))
            .ReturnsAsync(files);

        // Act
        var result = await _fileTools.ListFiles(directory);

        // Assert
        Assert.Contains("\"success\": true", result);
        Assert.Contains("file1.txt", result);
        Assert.Contains("file2.txt", result);
        Assert.Contains("\"fileCount\": 2", result);
    }

    [Fact]
    public async Task ListFiles_WithSearchPattern_UsesPattern()
    {
        // Arrange
        var directory = "c:\\test";
        var pattern = "*.log";
        var files = new List<FileMetadata>();
        _mockFileService.Setup(x => x.ListFilesAsync(directory, pattern, true))
            .ReturnsAsync(files);

        // Act
        var result = await _fileTools.ListFiles(directory, pattern);

        // Assert
        Assert.Contains("\"success\": true", result);
        Assert.Contains(pattern, result);
        _mockFileService.Verify(x => x.ListFilesAsync(directory, pattern, true), Times.Once);
    }

    [Fact]
    public async Task ListFiles_WhenServiceThrowsException_ReturnsError()
    {
        // Arrange
        var directory = "c:\\nonexistent";
        _mockFileService.Setup(x => x.ListFilesAsync(directory, "*", true))
            .ThrowsAsync(new System.IO.DirectoryNotFoundException());

        // Act
        var result = await _fileTools.ListFiles(directory);

        // Assert
        Assert.Contains("\"success\": false", result);
        Assert.Contains("error", result);
    }
}
