using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using filescrubberMCP.Services;
using filescrubberMCP.Interfaces;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Linq;

namespace filescrubberMCP.Tests.Services;

public class FileServiceTests : IDisposable
{
    private readonly Mock<ILogger<FileService>> _mockLogger;
    private readonly Mock<IAppConfigurationProvider> _mockConfigProvider;
    private readonly FileService _fileService;
    private readonly string _testDirectory;

    public FileServiceTests()
    {
        _mockLogger = new Mock<ILogger<FileService>>();
        _mockConfigProvider = new Mock<IAppConfigurationProvider>();
        _fileService = new FileService(_mockLogger.Object, _mockConfigProvider.Object);

        // Create temp directory for test files
        _testDirectory = Path.Combine(Path.GetTempPath(), "filescrubberMCP_Tests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
    }

    [Fact]
    public async Task ReadFileAsync_WithValidFile_ReturnsContent()
    {
        // Arrange
        var testFile = Path.Combine(_testDirectory, "test.txt");
        var expectedContent = "Test file content";
        await File.WriteAllTextAsync(testFile, expectedContent);

        // Act
        var result = await _fileService.ReadFileAsync(testFile);

        // Assert
        Assert.Equal(expectedContent, result);
    }

    [Fact]
    public async Task ReadFileAsync_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var nonExistentFile = Path.Combine(_testDirectory, "nonexistent.txt");

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() => _fileService.ReadFileAsync(nonExistentFile));
    }

    [Fact]
    public async Task WriteFileAsync_CreatesFileAndWritesContent()
    {
        // Arrange
        var testFile = Path.Combine(_testDirectory, "newfile.txt");
        var content = "New file content";

        // Act
        await _fileService.WriteFileAsync(testFile, content);

        // Assert
        Assert.True(File.Exists(testFile));
        var writtenContent = await File.ReadAllTextAsync(testFile);
        Assert.Equal(content, writtenContent);
    }

    [Fact]
    public async Task WriteFileAsync_CreatesDirectoryIfNotExists()
    {
        // Arrange
        var subDirectory = Path.Combine(_testDirectory, "subdir", "nested");
        var testFile = Path.Combine(subDirectory, "file.txt");
        var content = "Content in nested directory";

        // Act
        await _fileService.WriteFileAsync(testFile, content);

        // Assert
        Assert.True(Directory.Exists(subDirectory));
        Assert.True(File.Exists(testFile));
    }

    [Fact]
    public async Task ListFilesAsync_WithFiles_ReturnsMetadata()
    {
        // Arrange
        var file1 = Path.Combine(_testDirectory, "file1.txt");
        var file2 = Path.Combine(_testDirectory, "file2.txt");
        await File.WriteAllTextAsync(file1, "Content 1");
        await File.WriteAllTextAsync(file2, "Content 2");

        // Act
        var result = await _fileService.ListFilesAsync(_testDirectory, "*", false);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, f => f.file_name == "file1.txt");
        Assert.Contains(result, f => f.file_name == "file2.txt");
        Assert.All(result, f => Assert.True(f.size_in_bytes >= 0));
    }

    [Fact]
    public async Task ListFilesAsync_WithPattern_FiltersResults()
    {
        // Arrange
        await File.WriteAllTextAsync(Path.Combine(_testDirectory, "test.txt"), "Content");
        await File.WriteAllTextAsync(Path.Combine(_testDirectory, "test.log"), "Log content");
        await File.WriteAllTextAsync(Path.Combine(_testDirectory, "data.txt"), "Data");

        // Act
        var result = await _fileService.ListFilesAsync(_testDirectory, "*.txt", false);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, f => Assert.EndsWith(".txt", f.file_name));
    }

    [Fact]
    public async Task ListFilesAsync_Recursive_IncludesSubdirectories()
    {
        // Arrange
        var subDir = Path.Combine(_testDirectory, "subdir");
        Directory.CreateDirectory(subDir);
        await File.WriteAllTextAsync(Path.Combine(_testDirectory, "root.txt"), "Root");
        await File.WriteAllTextAsync(Path.Combine(subDir, "nested.txt"), "Nested");

        // Act
        var result = await _fileService.ListFilesAsync(_testDirectory, "*", true);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, f => f.file_name == "root.txt");
        Assert.Contains(result, f => f.file_name == "nested.txt");
    }

    [Fact]
    public async Task ListFilesAsync_EmptyDirectory_ReturnsEmptyList()
    {
        // Arrange
        var emptyDir = Path.Combine(_testDirectory, "empty");
        Directory.CreateDirectory(emptyDir);

        // Act
        var result = await _fileService.ListFilesAsync(emptyDir, "*", false);

        // Assert
        Assert.Empty(result);
    }

    public void Dispose()
    {
        // Cleanup test directory
        try
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}
