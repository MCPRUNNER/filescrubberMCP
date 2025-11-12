using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using filescrubberMCP.Services;
using filescrubberMCP.Interfaces;
using filescrubberMCP.Models;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace filescrubberMCP.Tests.Services;

public class WorkflowServiceTests
{
    private readonly Mock<ILogger<WorkflowService>> _mockLogger;
    private readonly Mock<IFileService> _mockFileService;
    private readonly Mock<IUriService> _mockUriService;
    private readonly Mock<ITemplateService> _mockTemplateService;
    private readonly Mock<IParserService> _mockParserService;
    private readonly Mock<IAIService> _mockAIService;
    private readonly WorkflowService _workflowService;

    public WorkflowServiceTests()
    {
        _mockLogger = new Mock<ILogger<WorkflowService>>();
        _mockFileService = new Mock<IFileService>();
        _mockUriService = new Mock<IUriService>();
        _mockTemplateService = new Mock<ITemplateService>();
        _mockParserService = new Mock<IParserService>();
        _mockAIService = new Mock<IAIService>();

        _workflowService = new WorkflowService(
            _mockLogger.Object,
            _mockFileService.Object,
            _mockUriService.Object,
            _mockTemplateService.Object,
            _mockParserService.Object,
            _mockAIService.Object
        );
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new WorkflowService(
                null!,
                _mockFileService.Object,
                _mockUriService.Object,
                _mockTemplateService.Object,
                _mockParserService.Object,
                _mockAIService.Object
            ));
    }

    [Fact]
    public void Constructor_WithNullFileService_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new WorkflowService(
                _mockLogger.Object,
                null!,
                _mockUriService.Object,
                _mockTemplateService.Object,
                _mockParserService.Object,
                _mockAIService.Object
            ));
    }

    #endregion

    #region LoadWorkflowAsync Tests

    [Fact]
    public async Task LoadWorkflowAsync_WithValidJson_ReturnsWorkflowDefinition()
    {
        // Arrange
        var workflowJson = JsonConvert.SerializeObject(new WorkflowDefinition
        {
            Steps = new List<WorkflowStep>
            {
                new WorkflowStep
                {
                    Name = "TestStep",
                    Type = "#fscrub_file_read",
                    Parameters = new Dictionary<string, object> { { "filePath", "test.txt" } },
                    Enabled = true
                }
            }
        });

        _mockFileService.Setup(s => s.ReadFileAsync("test-workflow.json"))
            .ReturnsAsync(workflowJson);

        // Act
        var result = await _workflowService.LoadWorkflowAsync("test-workflow.json");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Steps);
        Assert.Equal("TestStep", result.Steps[0].Name);
        Assert.Equal("#fscrub_file_read", result.Steps[0].Type);
    }

    [Fact]
    public async Task LoadWorkflowAsync_WithInvalidJson_ThrowsJsonException()
    {
        // Arrange
        _mockFileService.Setup(s => s.ReadFileAsync("invalid-workflow.json"))
            .ReturnsAsync("{ invalid json }");

        // Act & Assert
        await Assert.ThrowsAsync<JsonReaderException>(async () =>
            await _workflowService.LoadWorkflowAsync("invalid-workflow.json"));
    }

    [Fact]
    public async Task LoadWorkflowAsync_WithNonExistentFile_ThrowsException()
    {
        // Arrange
        _mockFileService.Setup(s => s.ReadFileAsync("nonexistent.json"))
            .ThrowsAsync(new System.IO.FileNotFoundException());

        // Act & Assert
        await Assert.ThrowsAsync<System.IO.FileNotFoundException>(async () =>
            await _workflowService.LoadWorkflowAsync("nonexistent.json"));
    }

    #endregion

    #region ExecuteWorkflowAsync Tests

    [Fact]
    public async Task ExecuteWorkflowAsync_WithSingleFileReadStep_ExecutesSuccessfully()
    {
        // Arrange
        var workflow = new WorkflowDefinition
        {
            Steps = new List<WorkflowStep>
            {
                new WorkflowStep
                {
                    Name = "ReadFile",
                    Type = "#fscrub_file_read",
                    Parameters = new Dictionary<string, object> { { "filePath", "test.txt" } },
                    Enabled = true
                }
            }
        };

        _mockFileService.Setup(s => s.ReadFileAsync("test.txt"))
            .ReturnsAsync("File content");

        // Act
        var result = await _workflowService.ExecuteWorkflowAsync(workflow);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorMessage);
        Assert.Single(result.StepResults);
        Assert.Equal("ReadFile", result.StepResults[0].StepName);
        Assert.True(result.StepResults[0].Success);
        Assert.Equal("File content", result.StepResults[0].Output);
    }

    [Fact]
    public async Task ExecuteWorkflowAsync_WithMultipleSteps_ExecutesInSequence()
    {
        // Arrange
        var workflow = new WorkflowDefinition
        {
            Steps = new List<WorkflowStep>
            {
                new WorkflowStep
                {
                    Name = "ReadFile",
                    Type = "#fscrub_file_read",
                    Parameters = new Dictionary<string, object> { { "filePath", "input.txt" } },
                    Enabled = true,
                    Output = new WorkflowStepOutput { Name = "Content", Format = "Text" }
                },
                new WorkflowStep
                {
                    Name = "WriteFile",
                    Type = "#fscrub_file_write",
                    Parameters = new Dictionary<string, object>
                    {
                        { "filePath", "output.txt" },
                        { "content", "{ReadFile.Content}" }
                    },
                    Enabled = true
                }
            }
        };

        _mockFileService.Setup(s => s.ReadFileAsync("input.txt"))
            .ReturnsAsync("Input content");
        _mockFileService.Setup(s => s.WriteFileAsync("output.txt", "Input content"))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _workflowService.ExecuteWorkflowAsync(workflow);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(2, result.StepResults.Count);
        Assert.All(result.StepResults, sr => Assert.True(sr.Success));
        _mockFileService.Verify(s => s.ReadFileAsync("input.txt"), Times.Once);
        _mockFileService.Verify(s => s.WriteFileAsync("output.txt", "Input content"), Times.Once);
    }

    [Fact]
    public async Task ExecuteWorkflowAsync_WithPlaceholderReplacement_ResolvesCorrectly()
    {
        // Arrange
        var workflow = new WorkflowDefinition
        {
            Steps = new List<WorkflowStep>
            {
                new WorkflowStep
                {
                    Name = "GetData",
                    Type = "#fscrub_uri_get",
                    Parameters = new Dictionary<string, object> { { "Uri", "https://api.example.com/data" } },
                    Enabled = true,
                    Output = new WorkflowStepOutput { Name = "Data", Format = "JSON" }
                },
                new WorkflowStep
                {
                    Name = "SaveData",
                    Type = "#fscrub_file_write",
                    Parameters = new Dictionary<string, object>
                    {
                        { "filePath", "data.json" },
                        { "content", "{GetData.Data}" }
                    },
                    Enabled = true
                }
            }
        };

        var apiResponse = "{\"key\":\"value\"}";
        _mockUriService.Setup(s => s.GetAsync("https://api.example.com/data", (Dictionary<string, string>?)null))
            .ReturnsAsync(apiResponse);
        _mockFileService.Setup(s => s.WriteFileAsync("data.json", It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _workflowService.ExecuteWorkflowAsync(workflow);

        // Assert
        Assert.True(result.Success);
        // The JSON may be formatted, so check for the key-value pair without exact formatting
        _mockFileService.Verify(s => s.WriteFileAsync("data.json", It.Is<string>(c => c.Contains("\"key\"") && c.Contains("\"value\""))), Times.Once);
    }

    [Fact]
    public async Task ExecuteWorkflowAsync_WithDisabledStep_SkipsStep()
    {
        // Arrange
        var workflow = new WorkflowDefinition
        {
            Steps = new List<WorkflowStep>
            {
                new WorkflowStep
                {
                    Name = "DisabledStep",
                    Type = "#fscrub_file_read",
                    Parameters = new Dictionary<string, object> { { "filePath", "test.txt" } },
                    Enabled = false
                },
                new WorkflowStep
                {
                    Name = "EnabledStep",
                    Type = "#fscrub_file_read",
                    Parameters = new Dictionary<string, object> { { "filePath", "other.txt" } },
                    Enabled = true
                }
            }
        };

        _mockFileService.Setup(s => s.ReadFileAsync("other.txt"))
            .ReturnsAsync("Other content");

        // Act
        var result = await _workflowService.ExecuteWorkflowAsync(workflow);

        // Assert
        Assert.True(result.Success);
        Assert.Single(result.StepResults);
        Assert.Equal("EnabledStep", result.StepResults[0].StepName);
        _mockFileService.Verify(s => s.ReadFileAsync("test.txt"), Times.Never);
        _mockFileService.Verify(s => s.ReadFileAsync("other.txt"), Times.Once);
    }

    [Fact]
    public async Task ExecuteWorkflowAsync_WithStepError_StopsExecution()
    {
        // Arrange
        var workflow = new WorkflowDefinition
        {
            Steps = new List<WorkflowStep>
            {
                new WorkflowStep
                {
                    Name = "FailingStep",
                    Type = "#fscrub_file_read",
                    Parameters = new Dictionary<string, object> { { "filePath", "nonexistent.txt" } },
                    Enabled = true
                },
                new WorkflowStep
                {
                    Name = "ShouldNotExecute",
                    Type = "#fscrub_file_read",
                    Parameters = new Dictionary<string, object> { { "filePath", "other.txt" } },
                    Enabled = true
                }
            }
        };

        _mockFileService.Setup(s => s.ReadFileAsync("nonexistent.txt"))
            .ThrowsAsync(new System.IO.FileNotFoundException("File not found"));

        // Act
        var result = await _workflowService.ExecuteWorkflowAsync(workflow);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("FailingStep", result.ErrorMessage);
        Assert.Single(result.StepResults);
        Assert.False(result.StepResults[0].Success);
        _mockFileService.Verify(s => s.ReadFileAsync("nonexistent.txt"), Times.Once);
        _mockFileService.Verify(s => s.ReadFileAsync("other.txt"), Times.Never);
    }

    [Fact]
    public async Task ExecuteWorkflowAsync_WithEmptyWorkflow_ReturnsSuccess()
    {
        // Arrange
        var workflow = new WorkflowDefinition
        {
            Steps = new List<WorkflowStep>()
        };

        // Act
        var result = await _workflowService.ExecuteWorkflowAsync(workflow);

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.StepResults);
    }

    #endregion

    #region File Operation Tests

    [Fact]
    public async Task ExecuteWorkflowAsync_WithFileWriteStep_WritesFile()
    {
        // Arrange
        var workflow = new WorkflowDefinition
        {
            Steps = new List<WorkflowStep>
            {
                new WorkflowStep
                {
                    Name = "WriteFile",
                    Type = "#fscrub_file_write",
                    Parameters = new Dictionary<string, object>
                    {
                        { "filePath", "output.txt" },
                        { "content", "Test content" }
                    },
                    Enabled = true
                }
            }
        };

        _mockFileService.Setup(s => s.WriteFileAsync("output.txt", "Test content"))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _workflowService.ExecuteWorkflowAsync(workflow);

        // Assert
        Assert.True(result.Success);
        _mockFileService.Verify(s => s.WriteFileAsync("output.txt", "Test content"), Times.Once);
    }

    [Fact]
    public async Task ExecuteWorkflowAsync_WithFileListStep_ListsFiles()
    {
        // Arrange
        var workflow = new WorkflowDefinition
        {
            Steps = new List<WorkflowStep>
            {
                new WorkflowStep
                {
                    Name = "ListFiles",
                    Type = "#fscrub_file_list",
                    Parameters = new Dictionary<string, object>
                    {
                        { "directoryPath", "C:\\TestDir" },
                        { "searchPattern", "*.txt" },
                        { "recursive", true }
                    },
                    Enabled = true
                }
            }
        };

        var fileList = new List<FileMetadata>
        {
            new FileMetadata { file_name = "test.txt", full_path = "C:\\TestDir\\test.txt" }
        };

        _mockFileService.Setup(s => s.ListFilesAsync("C:\\TestDir", "*.txt", true))
            .ReturnsAsync(fileList);

        // Act
        var result = await _workflowService.ExecuteWorkflowAsync(workflow);

        // Assert
        Assert.True(result.Success);
        _mockFileService.Verify(s => s.ListFilesAsync("C:\\TestDir", "*.txt", true), Times.Once);
    }

    #endregion

    #region URI Operation Tests

    [Fact]
    public async Task ExecuteWorkflowAsync_WithUriGetStep_FetchesData()
    {
        // Arrange
        var workflow = new WorkflowDefinition
        {
            Steps = new List<WorkflowStep>
            {
                new WorkflowStep
                {
                    Name = "FetchData",
                    Type = "#fscrub_uri_get",
                    Parameters = new Dictionary<string, object> { { "Uri", "https://api.example.com/data" } },
                    Enabled = true
                }
            }
        };

        _mockUriService.Setup(s => s.GetAsync("https://api.example.com/data", (Dictionary<string, string>?)null))
            .ReturnsAsync("{\"result\":\"success\"}");

        // Act
        var result = await _workflowService.ExecuteWorkflowAsync(workflow);

        // Assert
        Assert.True(result.Success);
        _mockUriService.Verify(s => s.GetAsync("https://api.example.com/data", (Dictionary<string, string>?)null), Times.Once);
    }

    [Fact]
    public async Task ExecuteWorkflowAsync_WithUriPostStep_PostsData()
    {
        // Arrange
        var workflow = new WorkflowDefinition
        {
            Steps = new List<WorkflowStep>
            {
                new WorkflowStep
                {
                    Name = "PostData",
                    Type = "#fscrub_uri_post",
                    Parameters = new Dictionary<string, object>
                    {
                        { "Uri", "https://api.example.com/create" },
                        { "jsonBody", "{\"name\":\"test\"}" }
                    },
                    Enabled = true
                }
            }
        };

        _mockUriService.Setup(s => s.PostAsync("https://api.example.com/create", "{\"name\":\"test\"}", (Dictionary<string, string>?)null))
            .ReturnsAsync("{\"id\":123}");

        // Act
        var result = await _workflowService.ExecuteWorkflowAsync(workflow);

        // Assert
        Assert.True(result.Success);
        _mockUriService.Verify(s => s.PostAsync("https://api.example.com/create", "{\"name\":\"test\"}", (Dictionary<string, string>?)null), Times.Once);
    }

    #endregion

    #region Template Operation Tests

    [Fact]
    public async Task ExecuteWorkflowAsync_WithTemplateProcessStep_ProcessesTemplate()
    {
        // Arrange
        var workflow = new WorkflowDefinition
        {
            Steps = new List<WorkflowStep>
            {
                new WorkflowStep
                {
                    Name = "ProcessTemplate",
                    Type = "#fscrub_scriban_process_template",
                    Parameters = new Dictionary<string, object>
                    {
                        { "templateFilePath", "template.sbn" },
                        { "jsonData", "{\"title\":\"Report\"}" },
                        { "outputFilePath", "output.txt" }
                    },
                    Enabled = true
                }
            }
        };

        _mockTemplateService.Setup(s => s.ProcessTemplateAsync("template.sbn", "{\"title\":\"Report\"}", "output.txt"))
            .ReturnsAsync("output.txt");

        // Act
        var result = await _workflowService.ExecuteWorkflowAsync(workflow);

        // Assert
        Assert.True(result.Success);
        _mockTemplateService.Verify(s => s.ProcessTemplateAsync("template.sbn", "{\"title\":\"Report\"}", "output.txt"), Times.Once);
    }

    [Fact]
    public async Task ExecuteWorkflowAsync_WithTemplateRenderStep_RendersTemplate()
    {
        // Arrange
        var workflow = new WorkflowDefinition
        {
            Steps = new List<WorkflowStep>
            {
                new WorkflowStep
                {
                    Name = "RenderTemplate",
                    Type = "#fscrub_scriban_render_template",
                    Parameters = new Dictionary<string, object>
                    {
                        { "templateFilePath", "template.sbn" },
                        { "jsonData", "{\"title\":\"Report\"}" }
                    },
                    Enabled = true
                }
            }
        };

        _mockTemplateService.Setup(s => s.RenderTemplateAsync("template.sbn", "{\"title\":\"Report\"}"))
            .ReturnsAsync("Rendered content");

        // Act
        var result = await _workflowService.ExecuteWorkflowAsync(workflow);

        // Assert
        Assert.True(result.Success);
        _mockTemplateService.Verify(s => s.RenderTemplateAsync("template.sbn", "{\"title\":\"Report\"}"), Times.Once);
    }

    #endregion

    #region Parser Operation Tests

    [Fact]
    public async Task ExecuteWorkflowAsync_WithJsonSearchStep_SearchesJson()
    {
        // Arrange
        var workflow = new WorkflowDefinition
        {
            Steps = new List<WorkflowStep>
            {
                new WorkflowStep
                {
                    Name = "SearchJson",
                    Type = "#fscrub_parser_search_json",
                    Parameters = new Dictionary<string, object>
                    {
                        { "jsonFilePath", "data.json" },
                        { "jsonPath", "$.users[*].name" }
                    },
                    Enabled = true
                }
            }
        };

        _mockParserService.Setup(s => s.SearchJsonFile("data.json", "$.users[*].name", true, false))
            .Returns("[\"Alice\",\"Bob\"]");

        // Act
        var result = await _workflowService.ExecuteWorkflowAsync(workflow);

        // Assert
        Assert.True(result.Success);
        _mockParserService.Verify(s => s.SearchJsonFile("data.json", "$.users[*].name", true, false), Times.Once);
    }

    [Fact]
    public async Task ExecuteWorkflowAsync_WithXmlSearchStep_SearchesXml()
    {
        // Arrange
        var workflow = new WorkflowDefinition
        {
            Steps = new List<WorkflowStep>
            {
                new WorkflowStep
                {
                    Name = "SearchXml",
                    Type = "#fscrub_parser_search_xml",
                    Parameters = new Dictionary<string, object>
                    {
                        { "xmlFilePath", "data.xml" },
                        { "xPath", "//user/@name" }
                    },
                    Enabled = true
                }
            }
        };

        _mockParserService.Setup(s => s.SearchXmlFile("data.xml", "//user/@name", true, false))
            .Returns("[\"Alice\",\"Bob\"]");

        // Act
        var result = await _workflowService.ExecuteWorkflowAsync(workflow);

        // Assert
        Assert.True(result.Success);
        _mockParserService.Verify(s => s.SearchXmlFile("data.xml", "//user/@name", true, false), Times.Once);
    }

    #endregion

    #region AI Operation Tests

    [Fact]
    public async Task ExecuteWorkflowAsync_WithAIStep_InvokesAIService()
    {
        // Arrange
        var workflow = new WorkflowDefinition
        {
            Steps = new List<WorkflowStep>
            {
                new WorkflowStep
                {
                    Name = "AskCopilot",
                    Type = "#fscrub_ask_github_copilot",
                    Parameters = new Dictionary<string, object>
                    {
                        { "prompt", "Analyze this data" }
                    },
                    Enabled = true
                }
            }
        };

        _mockAIService.Setup(s => s.AskGithubCopilotAsync("Analyze this data", null))
            .ReturnsAsync("AI analysis result");

        // Act
        var result = await _workflowService.ExecuteWorkflowAsync(workflow);

        // Assert
        Assert.True(result.Success);
        _mockAIService.Verify(s => s.AskGithubCopilotAsync("Analyze this data", null), Times.Once);
    }

    [Fact]
    public async Task ExecuteWorkflowAsync_WithAIStepAndPromptName_PassesPromptName()
    {
        // Arrange
        var workflow = new WorkflowDefinition
        {
            Steps = new List<WorkflowStep>
            {
                new WorkflowStep
                {
                    Name = "AskCopilot",
                    Type = "#fscrub_ask_github_copilot",
                    Parameters = new Dictionary<string, object>
                    {
                        { "prompt", "Analyze this data" },
                        { "promptName", "analysis.prompt.md" }
                    },
                    Enabled = true
                }
            }
        };

        _mockAIService.Setup(s => s.AskGithubCopilotAsync("Analyze this data", "analysis.prompt.md"))
            .ReturnsAsync("AI analysis result");

        // Act
        var result = await _workflowService.ExecuteWorkflowAsync(workflow);

        // Assert
        Assert.True(result.Success);
        _mockAIService.Verify(s => s.AskGithubCopilotAsync("Analyze this data", "analysis.prompt.md"), Times.Once);
    }

    #endregion

    #region ExecuteWorkflowFromFileAsync Tests

    [Fact]
    public async Task ExecuteWorkflowFromFileAsync_WithValidFile_LoadsAndExecutes()
    {
        // Arrange
        var workflowJson = JsonConvert.SerializeObject(new WorkflowDefinition
        {
            Steps = new List<WorkflowStep>
            {
                new WorkflowStep
                {
                    Name = "ReadFile",
                    Type = "#fscrub_file_read",
                    Parameters = new Dictionary<string, object> { { "filePath", "test.txt" } },
                    Enabled = true
                }
            }
        });

        _mockFileService.Setup(s => s.ReadFileAsync("workflow.json"))
            .ReturnsAsync(workflowJson);
        _mockFileService.Setup(s => s.ReadFileAsync("test.txt"))
            .ReturnsAsync("File content");

        // Act
        var result = await _workflowService.ExecuteWorkflowFromFileAsync("workflow.json");

        // Assert
        Assert.True(result.Success);
        Assert.Single(result.StepResults);
        _mockFileService.Verify(s => s.ReadFileAsync("workflow.json"), Times.Once);
        _mockFileService.Verify(s => s.ReadFileAsync("test.txt"), Times.Once);
    }

    [Fact]
    public async Task ExecuteWorkflowFromFileAsync_WithInvalidFile_ReturnsFailure()
    {
        // Arrange
        _mockFileService.Setup(s => s.ReadFileAsync("invalid.json"))
            .ThrowsAsync(new System.IO.FileNotFoundException());

        // Act
        var result = await _workflowService.ExecuteWorkflowFromFileAsync("invalid.json");

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
    }

    #endregion

    #region Unsupported Step Type Tests

    [Fact]
    public async Task ExecuteWorkflowAsync_WithUnsupportedStepType_ReturnsError()
    {
        // Arrange
        var workflow = new WorkflowDefinition
        {
            Steps = new List<WorkflowStep>
            {
                new WorkflowStep
                {
                    Name = "UnsupportedStep",
                    Type = "#fscrub_unsupported_operation",
                    Parameters = new Dictionary<string, object>(),
                    Enabled = true
                }
            }
        };

        // Act
        var result = await _workflowService.ExecuteWorkflowAsync(workflow);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("not supported", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Complex Workflow Tests

    [Fact]
    public async Task ExecuteWorkflowAsync_WithComplexDataPipeline_ExecutesCorrectly()
    {
        // Arrange
        var workflow = new WorkflowDefinition
        {
            Steps = new List<WorkflowStep>
            {
                new WorkflowStep
                {
                    Name = "FetchData",
                    Type = "#fscrub_uri_get",
                    Parameters = new Dictionary<string, object> { { "Uri", "https://api.example.com/data" } },
                    Enabled = true,
                    Output = new WorkflowStepOutput { Name = "JsonData", Format = "JSON" }
                },
                new WorkflowStep
                {
                    Name = "SaveRawData",
                    Type = "#fscrub_file_write",
                    Parameters = new Dictionary<string, object>
                    {
                        { "filePath", "raw_data.json" },
                        { "content", "{FetchData.JsonData}" }
                    },
                    Enabled = true
                },
                new WorkflowStep
                {
                    Name = "ProcessWithTemplate",
                    Type = "#fscrub_scriban_process_template",
                    Parameters = new Dictionary<string, object>
                    {
                        { "templateFilePath", "report.sbn" },
                        { "jsonData", "{FetchData.JsonData}" },
                        { "outputFilePath", "report.txt" }
                    },
                    Enabled = true,
                    Output = new WorkflowStepOutput { Name = "ReportPath", Format = "Text" }
                },
                new WorkflowStep
                {
                    Name = "ReadReport",
                    Type = "#fscrub_file_read",
                    Parameters = new Dictionary<string, object> { { "filePath", "{ProcessWithTemplate.ReportPath}" } },
                    Enabled = true
                }
            }
        };

        var apiData = "{\"employees\":[{\"name\":\"Alice\"}]}";
        _mockUriService.Setup(s => s.GetAsync("https://api.example.com/data", (Dictionary<string, string>?)null))
            .ReturnsAsync(apiData);
        _mockFileService.Setup(s => s.WriteFileAsync("raw_data.json", It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _mockTemplateService.Setup(s => s.ProcessTemplateAsync("report.sbn", It.IsAny<string>(), "report.txt"))
            .ReturnsAsync("report.txt");
        _mockFileService.Setup(s => s.ReadFileAsync("report.txt"))
            .ReturnsAsync("Employee Report: Alice");

        // Act
        var result = await _workflowService.ExecuteWorkflowAsync(workflow);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(4, result.StepResults.Count);
        Assert.All(result.StepResults, sr => Assert.True(sr.Success));

        // Verify execution order and parameters
        _mockUriService.Verify(s => s.GetAsync("https://api.example.com/data", (Dictionary<string, string>?)null), Times.Once);
        _mockFileService.Verify(s => s.WriteFileAsync("raw_data.json", It.IsAny<string>()), Times.Once);
        _mockTemplateService.Verify(s => s.ProcessTemplateAsync("report.sbn", It.IsAny<string>(), "report.txt"), Times.Once);
        _mockFileService.Verify(s => s.ReadFileAsync("report.txt"), Times.Once);
    }

    #endregion
}
