using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using filescrubberMCP.Services;
using filescrubberMCP.Interfaces;
using System.Threading.Tasks;
using System.IO;
using System;
using Newtonsoft.Json.Linq;

namespace filescrubberMCP.Tests.Services;

public class TemplateServiceTests : IDisposable
{
    private readonly Mock<ILogger<TemplateService>> _mockLogger;
    private readonly Mock<IFileService> _mockFileService;
    private readonly TemplateService _templateService;
    private readonly string _testDirectory;

    public TemplateServiceTests()
    {
        _mockLogger = new Mock<ILogger<TemplateService>>();
        _mockFileService = new Mock<IFileService>();
        _templateService = new TemplateService(_mockLogger.Object, _mockFileService.Object);

        // Create temp directory for test files
        _testDirectory = Path.Combine(Path.GetTempPath(), "TemplateService_Tests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
    }

    [Fact]
    public async Task ProcessTemplateAsync_WithValidTemplate_RendersSuccessfully()
    {
        // Arrange
        var templatePath = "template.sbn";
        var templateContent = "Hello {{ name }}!";
        var jsonData = "{\"name\": \"World\"}";
        var outputPath = "output.txt";
        var expectedOutput = "Hello World!";

        _mockFileService.Setup(x => x.ReadFileAsync(templatePath))
            .ReturnsAsync(templateContent);
        _mockFileService.Setup(x => x.WriteFileAsync(outputPath, expectedOutput))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _templateService.ProcessTemplateAsync(templatePath, jsonData, outputPath);

        // Assert
        Assert.Equal(outputPath, result);
        _mockFileService.Verify(x => x.ReadFileAsync(templatePath), Times.Once);
        _mockFileService.Verify(x => x.WriteFileAsync(outputPath, expectedOutput), Times.Once);
    }

    [Fact]
    public async Task ProcessTemplateAsync_WithJObject_RendersSuccessfully()
    {
        // Arrange
        var templatePath = "template.sbn";
        var templateContent = "Count: {{ items.size }}";
        var jsonData = new JObject
        {
            ["items"] = new JArray { 1, 2, 3 }
        };
        var outputPath = "output.txt";
        var expectedOutput = "Count: 3";

        _mockFileService.Setup(x => x.ReadFileAsync(templatePath))
            .ReturnsAsync(templateContent);
        _mockFileService.Setup(x => x.WriteFileAsync(outputPath, expectedOutput))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _templateService.ProcessTemplateAsync(templatePath, jsonData, outputPath);

        // Assert
        Assert.Equal(outputPath, result);
        _mockFileService.Verify(x => x.WriteFileAsync(outputPath, expectedOutput), Times.Once);
    }

    [Fact]
    public async Task ProcessTemplateAsync_WithLoop_RendersMultipleItems()
    {
        // Arrange
        var templatePath = "list.sbn";
        var templateContent = "{{ for item in items }}- {{ item }}\n{{ end }}";
        var jsonData = "{\"items\": [\"Apple\", \"Banana\", \"Cherry\"]}";
        var outputPath = "list.txt";
        var expectedOutput = "- Apple\n- Banana\n- Cherry\n";

        _mockFileService.Setup(x => x.ReadFileAsync(templatePath))
            .ReturnsAsync(templateContent);
        _mockFileService.Setup(x => x.WriteFileAsync(outputPath, expectedOutput))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _templateService.ProcessTemplateAsync(templatePath, jsonData, outputPath);

        // Assert
        Assert.Equal(outputPath, result);
        _mockFileService.Verify(x => x.WriteFileAsync(outputPath, expectedOutput), Times.Once);
    }

    [Fact]
    public async Task ProcessTemplateAsync_WithConditional_RendersCorrectly()
    {
        // Arrange
        var templatePath = "conditional.sbn";
        var templateContent = "{{ if show_message }}Message: {{ message }}{{ end }}";
        var jsonData = "{\"show_message\": true, \"message\": \"Hello\"}";
        var outputPath = "output.txt";
        var expectedOutput = "Message: Hello";

        _mockFileService.Setup(x => x.ReadFileAsync(templatePath))
            .ReturnsAsync(templateContent);
        _mockFileService.Setup(x => x.WriteFileAsync(outputPath, expectedOutput))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _templateService.ProcessTemplateAsync(templatePath, jsonData, outputPath);

        // Assert
        Assert.Equal(outputPath, result);
        _mockFileService.Verify(x => x.WriteFileAsync(outputPath, expectedOutput), Times.Once);
    }

    [Fact]
    public async Task ProcessTemplateAsync_WithNullTemplatePath_ReturnsError()
    {
        // Arrange
        string? templatePath = null;
        var jsonData = "{}";
        var outputPath = "output.txt";

        // Act
        var result = await _templateService.ProcessTemplateAsync(templatePath!, jsonData, outputPath);

        // Assert
        Assert.Contains("cannot be null or empty", result, StringComparison.OrdinalIgnoreCase);
        _mockFileService.Verify(x => x.ReadFileAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ProcessTemplateAsync_WithNullJsonData_ReturnsError()
    {
        // Arrange
        var templatePath = "template.sbn";
        object? jsonData = null;
        var outputPath = "output.txt";

        // Act
        var result = await _templateService.ProcessTemplateAsync(templatePath, jsonData!, outputPath);

        // Assert
        Assert.Contains("cannot be null", result, StringComparison.OrdinalIgnoreCase);
        _mockFileService.Verify(x => x.ReadFileAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ProcessTemplateAsync_WithNullOutputPath_ReturnsError()
    {
        // Arrange
        var templatePath = "template.sbn";
        var jsonData = "{}";
        string? outputPath = null;

        // Act
        var result = await _templateService.ProcessTemplateAsync(templatePath, jsonData, outputPath!);

        // Assert
        Assert.Contains("cannot be null or empty", result, StringComparison.OrdinalIgnoreCase);
        _mockFileService.Verify(x => x.ReadFileAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ProcessTemplateAsync_WithNonExistentTemplate_ReturnsError()
    {
        // Arrange
        var templatePath = "nonexistent.sbn";
        var jsonData = "{}";
        var outputPath = "output.txt";

        _mockFileService.Setup(x => x.ReadFileAsync(templatePath))
            .ThrowsAsync(new FileNotFoundException());

        // Act
        var result = await _templateService.ProcessTemplateAsync(templatePath, jsonData, outputPath);

        // Assert
        Assert.Contains("not found", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProcessTemplateAsync_WithEmptyTemplate_ReturnsError()
    {
        // Arrange
        var templatePath = "empty.sbn";
        var templateContent = "";
        var jsonData = "{}";
        var outputPath = "output.txt";

        _mockFileService.Setup(x => x.ReadFileAsync(templatePath))
            .ReturnsAsync(templateContent);

        // Act
        var result = await _templateService.ProcessTemplateAsync(templatePath, jsonData, outputPath);

        // Assert
        Assert.Contains("empty", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProcessTemplateAsync_WithInvalidTemplate_ReturnsError()
    {
        // Arrange
        var templatePath = "invalid.sbn";
        var templateContent = "{{ if true }}{{ end end }}"; // Invalid - duplicate end
        var jsonData = "{}";
        var outputPath = "output.txt";

        _mockFileService.Setup(x => x.ReadFileAsync(templatePath))
            .ReturnsAsync(templateContent);

        // Act
        var result = await _templateService.ProcessTemplateAsync(templatePath, jsonData, outputPath);

        // Assert
        Assert.Contains("parsing failed", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProcessTemplateAsync_WithInvalidJson_ReturnsError()
    {
        // Arrange
        var templatePath = "template.sbn";
        var templateContent = "Hello";
        var jsonData = "{invalid json}";
        var outputPath = "output.txt";

        _mockFileService.Setup(x => x.ReadFileAsync(templatePath))
            .ReturnsAsync(templateContent);

        // Act
        var result = await _templateService.ProcessTemplateAsync(templatePath, jsonData, outputPath);

        // Assert
        Assert.Contains("error", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RenderTemplateAsync_WithValidTemplate_ReturnsRenderedOutput()
    {
        // Arrange
        var templatePath = "template.sbn";
        var templateContent = "Result: {{ value }}";
        var jsonData = "{\"value\": 42}";
        var expectedOutput = "Result: 42";

        _mockFileService.Setup(x => x.ReadFileAsync(templatePath))
            .ReturnsAsync(templateContent);

        // Act
        var result = await _templateService.RenderTemplateAsync(templatePath, jsonData);

        // Assert
        Assert.Equal(expectedOutput, result);
        _mockFileService.Verify(x => x.ReadFileAsync(templatePath), Times.Once);
        _mockFileService.Verify(x => x.WriteFileAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RenderTemplateAsync_WithComplexTemplate_ReturnsCorrectOutput()
    {
        // Arrange
        var templatePath = "complex.sbn";
        var templateContent = @"
# {{ title }}
{{ for item in items }}
- {{ item.name }}: {{ item.value }}
{{ end }}
Total: {{ total }}
";
        var jsonData = @"{
            ""title"": ""Report"",
            ""items"": [
                {""name"": ""A"", ""value"": 10},
                {""name"": ""B"", ""value"": 20}
            ],
            ""total"": 30
        }";

        _mockFileService.Setup(x => x.ReadFileAsync(templatePath))
            .ReturnsAsync(templateContent);

        // Act
        var result = await _templateService.RenderTemplateAsync(templatePath, jsonData);

        // Assert
        Assert.Contains("# Report", result);
        Assert.Contains("- A: 10", result);
        Assert.Contains("- B: 20", result);
        Assert.Contains("Total: 30", result);
    }

    [Fact]
    public async Task RenderTemplateAsync_WithNullTemplatePath_ThrowsException()
    {
        // Arrange
        string? templatePath = null;
        var jsonData = "{}";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _templateService.RenderTemplateAsync(templatePath!, jsonData));
    }

    [Fact]
    public async Task RenderTemplateAsync_WithNullJsonData_ThrowsException()
    {
        // Arrange
        var templatePath = "template.sbn";
        object? jsonData = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _templateService.RenderTemplateAsync(templatePath, jsonData!));
    }

    [Fact]
    public async Task RenderTemplateAsync_WithNonExistentTemplate_ThrowsException()
    {
        // Arrange
        var templatePath = "nonexistent.sbn";
        var jsonData = "{}";

        _mockFileService.Setup(x => x.ReadFileAsync(templatePath))
            .ThrowsAsync(new FileNotFoundException());

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            _templateService.RenderTemplateAsync(templatePath, jsonData));
    }

    [Fact]
    public async Task RenderTemplateAsync_WithEmptyTemplate_ThrowsException()
    {
        // Arrange
        var templatePath = "empty.sbn";
        var templateContent = "";
        var jsonData = "{}";

        _mockFileService.Setup(x => x.ReadFileAsync(templatePath))
            .ReturnsAsync(templateContent);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _templateService.RenderTemplateAsync(templatePath, jsonData));
    }

    [Fact]
    public async Task RenderTemplateAsync_WithInvalidTemplate_ThrowsException()
    {
        // Arrange
        var templatePath = "invalid.sbn";
        var templateContent = "{{ if true }}{{ end end }}"; // Invalid - duplicate end
        var jsonData = "{}";

        _mockFileService.Setup(x => x.ReadFileAsync(templatePath))
            .ReturnsAsync(templateContent);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _templateService.RenderTemplateAsync(templatePath, jsonData));
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
