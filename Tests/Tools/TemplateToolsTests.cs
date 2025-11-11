using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using filescrubberMCP.Tools;
using filescrubberMCP.Interfaces;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace filescrubberMCP.Tests.Tools;

public class TemplateToolsTests
{
    private readonly Mock<ILogger<TemplateTools>> _mockLogger;
    private readonly Mock<ITemplateService> _mockTemplateService;
    private readonly TemplateTools _templateTools;

    public TemplateToolsTests()
    {
        _mockLogger = new Mock<ILogger<TemplateTools>>();
        _mockTemplateService = new Mock<ITemplateService>();
        _templateTools = new TemplateTools(_mockLogger.Object, _mockTemplateService.Object);
    }

    [Fact]
    public async Task ProcessTemplate_WithValidInput_ReturnsSuccess()
    {
        // Arrange
        var templatePath = "template.sbn";
        var jsonData = "{\"name\": \"Test\"}";
        var outputPath = "output.txt";
        var expectedOutputPath = outputPath; // Service returns the path as-is

        _mockTemplateService.Setup(x => x.ProcessTemplateAsync(templatePath, jsonData, outputPath))
            .ReturnsAsync(expectedOutputPath);

        // Act
        var result = await _templateTools.ProcessTemplate(templatePath, jsonData, outputPath);

        // Assert
        Assert.Contains("\"success\": true", result);
        Assert.Contains(expectedOutputPath, result);
        Assert.Contains("processed successfully", result);
        _mockTemplateService.Verify(x => x.ProcessTemplateAsync(templatePath, jsonData, outputPath), Times.Once);
    }

    [Fact]
    public async Task ProcessTemplate_WhenServiceReturnsError_ReturnsFailure()
    {
        // Arrange
        var templatePath = "template.sbn";
        var jsonData = "{}";
        var outputPath = "output.txt";
        var errorMessage = "Template file not found: template.sbn";

        _mockTemplateService.Setup(x => x.ProcessTemplateAsync(templatePath, jsonData, outputPath))
            .ReturnsAsync(errorMessage);

        // Act
        var result = await _templateTools.ProcessTemplate(templatePath, jsonData, outputPath);

        // Assert
        Assert.Contains("\"success\": false", result);
        Assert.Contains(errorMessage, result);
        Assert.Contains("error", result);
    }

    [Fact]
    public async Task ProcessTemplate_WhenServiceThrowsException_ReturnsError()
    {
        // Arrange
        var templatePath = "template.sbn";
        var jsonData = "{}";
        var outputPath = "output.txt";
        var exceptionMessage = "Unexpected error occurred";

        _mockTemplateService.Setup(x => x.ProcessTemplateAsync(templatePath, jsonData, outputPath))
            .ThrowsAsync(new System.Exception(exceptionMessage));

        // Act
        var result = await _templateTools.ProcessTemplate(templatePath, jsonData, outputPath);

        // Assert
        Assert.Contains("\"success\": false", result);
        Assert.Contains(exceptionMessage, result);
    }

    [Fact]
    public async Task ProcessTemplate_WithComplexJson_ProcessesCorrectly()
    {
        // Arrange
        var templatePath = "report.sbn";
        var jsonData = @"{
            ""title"": ""Monthly Report"",
            ""items"": [
                {""name"": ""Item 1"", ""value"": 100},
                {""name"": ""Item 2"", ""value"": 200}
            ]
        }";
        var outputPath = "report.md";
        var expectedOutputPath = outputPath; // Service returns the path as-is

        _mockTemplateService.Setup(x => x.ProcessTemplateAsync(templatePath, jsonData, outputPath))
            .ReturnsAsync(expectedOutputPath);

        // Act
        var result = await _templateTools.ProcessTemplate(templatePath, jsonData, outputPath);

        // Assert
        Assert.Contains("\"success\": true", result);
        Assert.Contains(expectedOutputPath, result);
        _mockTemplateService.Verify(x => x.ProcessTemplateAsync(templatePath, jsonData, outputPath), Times.Once);
    }

    [Fact]
    public async Task ProcessTemplate_WithFileListData_ProcessesCorrectly()
    {
        // Arrange
        var templatePath = "Examples/file_list_report.sbn";
        var jsonData = @"{
            ""success"": true,
            ""directoryPath"": ""C:\\Projects"",
            ""searchPattern"": ""*.cs"",
            ""recursive"": true,
            ""fileCount"": 2,
            ""files"": [
                {
                    ""file_name"": ""Program.cs"",
                    ""full_path"": ""C:\\Projects\\Program.cs"",
                    ""size_in_bytes"": 1024,
                    ""extension"": "".cs""
                },
                {
                    ""file_name"": ""Startup.cs"",
                    ""full_path"": ""C:\\Projects\\Startup.cs"",
                    ""size_in_bytes"": 2048,
                    ""extension"": "".cs""
                }
            ]
        }";
        var outputPath = "file_report.md";
        var expectedOutputPath = outputPath; // Service returns the path as-is

        _mockTemplateService.Setup(x => x.ProcessTemplateAsync(templatePath, jsonData, outputPath))
            .ReturnsAsync(expectedOutputPath);

        // Act
        var result = await _templateTools.ProcessTemplate(templatePath, jsonData, outputPath);

        // Assert
        Assert.Contains("\"success\": true", result);
        Assert.Contains(expectedOutputPath, result);
    }

    [Fact]
    public async Task RenderTemplate_WithValidInput_ReturnsSuccess()
    {
        // Arrange
        var templatePath = "template.sbn";
        var jsonData = "{\"greeting\": \"Hello World\"}";
        var renderedOutput = "Hello World";

        _mockTemplateService.Setup(x => x.RenderTemplateAsync(templatePath, jsonData))
            .ReturnsAsync(renderedOutput);

        // Act
        var result = await _templateTools.RenderTemplate(templatePath, jsonData);

        // Assert
        Assert.Contains("\"success\": true", result);
        Assert.Contains(renderedOutput, result);
        Assert.Contains("renderedOutput", result);
        Assert.Contains("outputLength", result);
        _mockTemplateService.Verify(x => x.RenderTemplateAsync(templatePath, jsonData), Times.Once);
    }

    [Fact]
    public async Task RenderTemplate_WithMultilineOutput_ReturnsSuccess()
    {
        // Arrange
        var templatePath = "list.sbn";
        var jsonData = "{\"items\": [\"A\", \"B\", \"C\"]}";
        var renderedOutput = "- A\n- B\n- C\n";

        _mockTemplateService.Setup(x => x.RenderTemplateAsync(templatePath, jsonData))
            .ReturnsAsync(renderedOutput);

        // Act
        var result = await _templateTools.RenderTemplate(templatePath, jsonData);

        // Assert
        Assert.Contains("\"success\": true", result);
        Assert.Contains("\"outputLength\": " + renderedOutput.Length, result);
        // Note: renderedOutput will be escaped in JSON
        var resultObj = JObject.Parse(result);
        Assert.Equal(renderedOutput, resultObj["renderedOutput"]?.ToString());
    }

    [Fact]
    public async Task RenderTemplate_WhenServiceThrowsException_ReturnsError()
    {
        // Arrange
        var templatePath = "template.sbn";
        var jsonData = "{}";
        var exceptionMessage = "Template parsing failed";

        _mockTemplateService.Setup(x => x.RenderTemplateAsync(templatePath, jsonData))
            .ThrowsAsync(new System.InvalidOperationException(exceptionMessage));

        // Act
        var result = await _templateTools.RenderTemplate(templatePath, jsonData);

        // Assert
        Assert.Contains("\"success\": false", result);
        Assert.Contains(exceptionMessage, result);
    }

    [Fact]
    public async Task RenderTemplate_WithEmptyOutput_ReturnsSuccess()
    {
        // Arrange
        var templatePath = "empty.sbn";
        var jsonData = "{\"show\": false}";
        var renderedOutput = "";

        _mockTemplateService.Setup(x => x.RenderTemplateAsync(templatePath, jsonData))
            .ReturnsAsync(renderedOutput);

        // Act
        var result = await _templateTools.RenderTemplate(templatePath, jsonData);

        // Assert
        Assert.Contains("\"success\": true", result);
        Assert.Contains("\"outputLength\": 0", result);
    }

    [Fact]
    public async Task RenderTemplate_WithLargeOutput_ReturnsSuccess()
    {
        // Arrange
        var templatePath = "large.sbn";
        var jsonData = "{\"count\": 1000}";
        var renderedOutput = new string('*', 10000); // Large output

        _mockTemplateService.Setup(x => x.RenderTemplateAsync(templatePath, jsonData))
            .ReturnsAsync(renderedOutput);

        // Act
        var result = await _templateTools.RenderTemplate(templatePath, jsonData);

        // Assert
        Assert.Contains("\"success\": true", result);
        Assert.Contains("\"outputLength\": 10000", result);
    }

    [Fact]
    public async Task ProcessTemplate_ReturnsValidJson()
    {
        // Arrange
        var templatePath = "template.sbn";
        var jsonData = "{}";
        var outputPath = "output.txt";
        var expectedOutputPath = "output.txt";

        _mockTemplateService.Setup(x => x.ProcessTemplateAsync(templatePath, jsonData, outputPath))
            .ReturnsAsync(expectedOutputPath);

        // Act
        var result = await _templateTools.ProcessTemplate(templatePath, jsonData, outputPath);

        // Assert - Verify it's valid JSON
        var parsed = JObject.Parse(result);
        Assert.NotNull(parsed);
        Assert.True(parsed.ContainsKey("success"));
    }

    [Fact]
    public async Task RenderTemplate_ReturnsValidJson()
    {
        // Arrange
        var templatePath = "template.sbn";
        var jsonData = "{}";
        var renderedOutput = "Output";

        _mockTemplateService.Setup(x => x.RenderTemplateAsync(templatePath, jsonData))
            .ReturnsAsync(renderedOutput);

        // Act
        var result = await _templateTools.RenderTemplate(templatePath, jsonData);

        // Assert - Verify it's valid JSON
        var parsed = JObject.Parse(result);
        Assert.NotNull(parsed);
        Assert.True(parsed.ContainsKey("success"));
        Assert.True(parsed.ContainsKey("renderedOutput"));
    }
}
