using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using filescrubberMCP.Tools;
using filescrubberMCP.Interfaces;
using filescrubberMCP.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace filescrubberMCP.Tests.Tools;

public class WorkflowToolsTests
{
    private readonly Mock<ILogger<WorkflowTools>> _mockLogger;
    private readonly Mock<IWorkflowService> _mockWorkflowService;
    private readonly WorkflowTools _workflowTools;

    public WorkflowToolsTests()
    {
        _mockLogger = new Mock<ILogger<WorkflowTools>>();
        _mockWorkflowService = new Mock<IWorkflowService>();
        _workflowTools = new WorkflowTools(_mockLogger.Object, _mockWorkflowService.Object);
    }

    [Fact]
    public async Task ExecuteWorkflow_WithValidWorkflow_ReturnsSuccessJson()
    {
        // Arrange
        var workflowPath = ".fscrub/workflows/test.json";
        var workflowResult = new WorkflowResult
        {
            Success = true,
            StepResults = new List<WorkflowStepResult>
            {
                new WorkflowStepResult
                {
                    StepName = "Step1",
                    Success = true,
                    Output = "Step 1 output",
                    ExecutionTimeMs = 100
                }
            },
            StepOutputs = new Dictionary<string, object>
            {
                { "Step1", "Step 1 output" }
            }
        };

        _mockWorkflowService.Setup(s => s.ExecuteWorkflowFromFileAsync(workflowPath))
            .ReturnsAsync(workflowResult);

        // Act
        var result = await _workflowTools.ExecuteWorkflow(workflowPath);

        // Assert
        var json = JObject.Parse(result);
        Assert.True((bool)json["success"]!);
        Assert.Null((string?)json["errorMessage"]);
        Assert.NotNull(json["stepResults"]);
        Assert.Single((JArray)json["stepResults"]!);

        var stepResult = (JObject)json["stepResults"]![0]!;
        Assert.Equal("Step1", (string)stepResult["stepName"]!);
        Assert.True((bool)stepResult["success"]!);
        Assert.Equal(100, (long)stepResult["executionTimeMs"]!);

        _mockWorkflowService.Verify(s => s.ExecuteWorkflowFromFileAsync(workflowPath), Times.Once);
    }

    [Fact]
    public async Task ExecuteWorkflow_WithFailedWorkflow_ReturnsFailureJson()
    {
        // Arrange
        var workflowPath = ".fscrub/workflows/failing.json";
        var workflowResult = new WorkflowResult
        {
            Success = false,
            ErrorMessage = "Step 'FailingStep' failed: File not found",
            StepResults = new List<WorkflowStepResult>
            {
                new WorkflowStepResult
                {
                    StepName = "FailingStep",
                    Success = false,
                    ErrorMessage = "File not found",
                    ExecutionTimeMs = 50
                }
            }
        };

        _mockWorkflowService.Setup(s => s.ExecuteWorkflowFromFileAsync(workflowPath))
            .ReturnsAsync(workflowResult);

        // Act
        var result = await _workflowTools.ExecuteWorkflow(workflowPath);

        // Assert
        var json = JObject.Parse(result);
        Assert.False((bool)json["success"]!);
        Assert.NotNull((string?)json["errorMessage"]);
        Assert.Contains("FailingStep", (string)json["errorMessage"]!);
        Assert.Contains("File not found", (string)json["errorMessage"]!);

        var stepResults = (JArray)json["stepResults"]!;
        Assert.Single(stepResults);
        var stepResult = (JObject)stepResults[0]!;
        Assert.False((bool)stepResult["success"]!);
        Assert.Equal("FailingStep", (string)stepResult["stepName"]!);

        _mockWorkflowService.Verify(s => s.ExecuteWorkflowFromFileAsync(workflowPath), Times.Once);
    }

    [Fact]
    public async Task ExecuteWorkflow_WithMultipleSteps_ReturnsAllStepResults()
    {
        // Arrange
        var workflowPath = ".fscrub/workflows/multi-step.json";
        var workflowResult = new WorkflowResult
        {
            Success = true,
            StepResults = new List<WorkflowStepResult>
            {
                new WorkflowStepResult
                {
                    StepName = "FetchData",
                    Success = true,
                    Output = "{\"data\":\"value\"}",
                    ExecutionTimeMs = 150
                },
                new WorkflowStepResult
                {
                    StepName = "ProcessData",
                    Success = true,
                    Output = "Processed: value",
                    ExecutionTimeMs = 75
                },
                new WorkflowStepResult
                {
                    StepName = "SaveData",
                    Success = true,
                    Output = "Successfully wrote to file: output.txt",
                    ExecutionTimeMs = 25
                }
            },
            StepOutputs = new Dictionary<string, object>
            {
                { "FetchData", "{\"data\":\"value\"}" },
                { "ProcessData", "Processed: value" },
                { "SaveData", "Successfully wrote to file: output.txt" }
            }
        };

        _mockWorkflowService.Setup(s => s.ExecuteWorkflowFromFileAsync(workflowPath))
            .ReturnsAsync(workflowResult);

        // Act
        var result = await _workflowTools.ExecuteWorkflow(workflowPath);

        // Assert
        var json = JObject.Parse(result);
        Assert.True((bool)json["success"]!);

        var stepResults = (JArray)json["stepResults"]!;
        Assert.Equal(3, stepResults.Count);

        Assert.Equal("FetchData", (string)stepResults[0]!["stepName"]!);
        Assert.Equal("ProcessData", (string)stepResults[1]!["stepName"]!);
        Assert.Equal("SaveData", (string)stepResults[2]!["stepName"]!);

        Assert.All(stepResults.Cast<JObject>(), sr => Assert.True((bool)sr["success"]!));

        var stepOutputs = (JObject)json["stepOutputs"]!;
        Assert.Equal(3, stepOutputs.Count);
        Assert.NotNull(stepOutputs["FetchData"]);
        Assert.NotNull(stepOutputs["ProcessData"]);
        Assert.NotNull(stepOutputs["SaveData"]);

        _mockWorkflowService.Verify(s => s.ExecuteWorkflowFromFileAsync(workflowPath), Times.Once);
    }

    [Fact]
    public async Task ExecuteWorkflow_WithEmptyWorkflow_ReturnsSuccessWithNoSteps()
    {
        // Arrange
        var workflowPath = ".fscrub/workflows/empty.json";
        var workflowResult = new WorkflowResult
        {
            Success = true,
            StepResults = new List<WorkflowStepResult>(),
            StepOutputs = new Dictionary<string, object>()
        };

        _mockWorkflowService.Setup(s => s.ExecuteWorkflowFromFileAsync(workflowPath))
            .ReturnsAsync(workflowResult);

        // Act
        var result = await _workflowTools.ExecuteWorkflow(workflowPath);

        // Assert
        var json = JObject.Parse(result);
        Assert.True((bool)json["success"]!);
        Assert.Null((string?)json["errorMessage"]);

        var stepResults = (JArray)json["stepResults"]!;
        Assert.Empty(stepResults);

        var stepOutputs = (JObject)json["stepOutputs"]!;
        Assert.Empty(stepOutputs);

        _mockWorkflowService.Verify(s => s.ExecuteWorkflowFromFileAsync(workflowPath), Times.Once);
    }

    [Fact]
    public async Task ExecuteWorkflow_WhenServiceThrowsException_ReturnsErrorJson()
    {
        // Arrange
        var workflowPath = ".fscrub/workflows/nonexistent.json";
        var errorMessage = "Workflow file not found";

        _mockWorkflowService.Setup(s => s.ExecuteWorkflowFromFileAsync(workflowPath))
            .ThrowsAsync(new System.IO.FileNotFoundException(errorMessage));

        // Act
        var result = await _workflowTools.ExecuteWorkflow(workflowPath);

        // Assert
        var json = JObject.Parse(result);
        Assert.False((bool)json["success"]!);
        Assert.NotNull((string?)json["errorMessage"]);
        Assert.Contains(errorMessage, (string)json["errorMessage"]!);
        Assert.Equal(workflowPath, (string)json["workflowFilePath"]!);

        _mockWorkflowService.Verify(s => s.ExecuteWorkflowFromFileAsync(workflowPath), Times.Once);
    }

    [Fact]
    public async Task ExecuteWorkflow_WithStepExecutionTimes_ReturnsTimingInformation()
    {
        // Arrange
        var workflowPath = ".fscrub/workflows/timed.json";
        var workflowResult = new WorkflowResult
        {
            Success = true,
            StepResults = new List<WorkflowStepResult>
            {
                new WorkflowStepResult
                {
                    StepName = "SlowStep",
                    Success = true,
                    Output = "Slow output",
                    ExecutionTimeMs = 1500
                },
                new WorkflowStepResult
                {
                    StepName = "FastStep",
                    Success = true,
                    Output = "Fast output",
                    ExecutionTimeMs = 10
                }
            }
        };

        _mockWorkflowService.Setup(s => s.ExecuteWorkflowFromFileAsync(workflowPath))
            .ReturnsAsync(workflowResult);

        // Act
        var result = await _workflowTools.ExecuteWorkflow(workflowPath);

        // Assert
        var json = JObject.Parse(result);
        var stepResults = (JArray)json["stepResults"]!;

        Assert.Equal(1500, (long)stepResults[0]!["executionTimeMs"]!);
        Assert.Equal(10, (long)stepResults[1]!["executionTimeMs"]!);

        _mockWorkflowService.Verify(s => s.ExecuteWorkflowFromFileAsync(workflowPath), Times.Once);
    }

    [Fact]
    public async Task ExecuteWorkflow_WithComplexOutputs_SerializesCorrectly()
    {
        // Arrange
        var workflowPath = ".fscrub/workflows/complex.json";
        var complexOutput = new
        {
            users = new[]
            {
                new { id = 1, name = "Alice", roles = new[] { "admin", "user" } },
                new { id = 2, name = "Bob", roles = new[] { "user" } }
            },
            metadata = new
            {
                version = "1.0",
                timestamp = "2025-01-01T00:00:00Z"
            }
        };

        var workflowResult = new WorkflowResult
        {
            Success = true,
            StepResults = new List<WorkflowStepResult>
            {
                new WorkflowStepResult
                {
                    StepName = "ComplexStep",
                    Success = true,
                    Output = complexOutput,
                    ExecutionTimeMs = 200
                }
            },
            StepOutputs = new Dictionary<string, object>
            {
                { "ComplexStep", complexOutput }
            }
        };

        _mockWorkflowService.Setup(s => s.ExecuteWorkflowFromFileAsync(workflowPath))
            .ReturnsAsync(workflowResult);

        // Act
        var result = await _workflowTools.ExecuteWorkflow(workflowPath);

        // Assert
        var json = JObject.Parse(result);
        Assert.True((bool)json["success"]!);

        var stepResult = (JObject)json["stepResults"]![0]!;
        var output = (JObject)stepResult["output"]!;
        Assert.NotNull(output);
        Assert.NotNull(output["users"]);
        Assert.NotNull(output["metadata"]);

        var stepOutputs = (JObject)json["stepOutputs"]!;
        var complexStepOutput = (JObject)stepOutputs["ComplexStep"]!;
        Assert.NotNull(complexStepOutput);
        Assert.NotNull(complexStepOutput["users"]);

        _mockWorkflowService.Verify(s => s.ExecuteWorkflowFromFileAsync(workflowPath), Times.Once);
    }

    [Fact]
    public async Task ExecuteWorkflow_WithPartialFailure_ReturnsCorrectStatus()
    {
        // Arrange
        var workflowPath = ".fscrub/workflows/partial.json";
        var workflowResult = new WorkflowResult
        {
            Success = false,
            ErrorMessage = "Step 'SecondStep' failed: Invalid data format",
            StepResults = new List<WorkflowStepResult>
            {
                new WorkflowStepResult
                {
                    StepName = "FirstStep",
                    Success = true,
                    Output = "First step succeeded",
                    ExecutionTimeMs = 100
                },
                new WorkflowStepResult
                {
                    StepName = "SecondStep",
                    Success = false,
                    ErrorMessage = "Invalid data format",
                    ExecutionTimeMs = 50
                }
            },
            StepOutputs = new Dictionary<string, object>
            {
                { "FirstStep", "First step succeeded" }
            }
        };

        _mockWorkflowService.Setup(s => s.ExecuteWorkflowFromFileAsync(workflowPath))
            .ReturnsAsync(workflowResult);

        // Act
        var result = await _workflowTools.ExecuteWorkflow(workflowPath);

        // Assert
        var json = JObject.Parse(result);
        Assert.False((bool)json["success"]!);
        Assert.Contains("SecondStep", (string)json["errorMessage"]!);

        var stepResults = (JArray)json["stepResults"]!;
        Assert.Equal(2, stepResults.Count);

        var firstStep = (JObject)stepResults[0]!;
        Assert.True((bool)firstStep["success"]!);
        Assert.Null((string?)firstStep["errorMessage"]);

        var secondStep = (JObject)stepResults[1]!;
        Assert.False((bool)secondStep["success"]!);
        Assert.NotNull((string?)secondStep["errorMessage"]);

        _mockWorkflowService.Verify(s => s.ExecuteWorkflowFromFileAsync(workflowPath), Times.Once);
    }

    [Fact]
    public async Task ExecuteWorkflow_WithNullStepOutput_HandlesGracefully()
    {
        // Arrange
        var workflowPath = ".fscrub/workflows/null-output.json";
        var workflowResult = new WorkflowResult
        {
            Success = true,
            StepResults = new List<WorkflowStepResult>
            {
                new WorkflowStepResult
                {
                    StepName = "NullOutputStep",
                    Success = true,
                    Output = null,
                    ExecutionTimeMs = 10
                }
            },
            StepOutputs = new Dictionary<string, object>
            {
                { "NullOutputStep", null! }
            }
        };

        _mockWorkflowService.Setup(s => s.ExecuteWorkflowFromFileAsync(workflowPath))
            .ReturnsAsync(workflowResult);

        // Act
        var result = await _workflowTools.ExecuteWorkflow(workflowPath);

        // Assert
        var json = JObject.Parse(result);
        Assert.True((bool)json["success"]!);

        var stepResult = (JObject)json["stepResults"]![0]!;
        Assert.True((bool)stepResult["success"]!);
        // Output can be null in JSON
        Assert.True(stepResult["output"]!.Type == JTokenType.Null || stepResult["output"] == null);

        _mockWorkflowService.Verify(s => s.ExecuteWorkflowFromFileAsync(workflowPath), Times.Once);
    }

    [Fact]
    public async Task ExecuteWorkflow_LogsAppropriateInformation()
    {
        // Arrange
        var workflowPath = ".fscrub/workflows/logging-test.json";
        var workflowResult = new WorkflowResult
        {
            Success = true,
            StepResults = new List<WorkflowStepResult>()
        };

        _mockWorkflowService.Setup(s => s.ExecuteWorkflowFromFileAsync(workflowPath))
            .ReturnsAsync(workflowResult);

        // Act
        await _workflowTools.ExecuteWorkflow(workflowPath);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ExecuteWorkflow tool called")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteWorkflow_WhenExceptionThrown_LogsError()
    {
        // Arrange
        var workflowPath = ".fscrub/workflows/error.json";
        var exception = new InvalidOperationException("Critical error");

        _mockWorkflowService.Setup(s => s.ExecuteWorkflowFromFileAsync(workflowPath))
            .ThrowsAsync(exception);

        // Act
        await _workflowTools.ExecuteWorkflow(workflowPath);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error in ExecuteWorkflow tool")),
                It.Is<Exception>(e => e == exception),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
