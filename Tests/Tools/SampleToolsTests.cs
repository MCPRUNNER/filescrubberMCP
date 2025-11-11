using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using filescrubberMCP.Tools;
using filescrubberMCP.Interfaces;
using System.Threading.Tasks;
using System;

namespace filescrubberMCP.Tests.Tools;

public class SampleToolsTests
{
    private readonly Mock<ILogger<SampleTools>> _mockLogger;
    private readonly Mock<ISampleService> _mockSampleService;
    private readonly SampleTools _sampleTools;

    public SampleToolsTests()
    {
        _mockLogger = new Mock<ILogger<SampleTools>>();
        _mockSampleService = new Mock<ISampleService>();
        _sampleTools = new SampleTools(_mockLogger.Object, _mockSampleService.Object);
    }

    [Fact]
    public async Task Initialize_WithValidConnectionName_ReturnsSuccess()
    {
        // Arrange
        var connectionName = "TestConnection";
        var expectedResult = new
        {
            success = true,
            message = $"Connection '{connectionName}' initialized successfully",
            connectionName = connectionName
        };
        _mockSampleService.Setup(x => x.InitializeConnectionAsync(connectionName))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _sampleTools.Initialize(connectionName);

        // Assert
        Assert.Contains("success", result);
        Assert.Contains(connectionName, result);
        _mockSampleService.Verify(x => x.InitializeConnectionAsync(connectionName), Times.Once);
    }

    [Fact]
    public async Task Initialize_WithDefaultConnectionName_ReturnsSuccess()
    {
        // Arrange
        var expectedResult = new
        {
            success = true,
            message = "Connection 'DefaultConnection' initialized successfully",
            connectionName = "DefaultConnection"
        };
        _mockSampleService.Setup(x => x.InitializeConnectionAsync("DefaultConnection"))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _sampleTools.Initialize();

        // Assert
        Assert.Contains("success", result);
        Assert.Contains("DefaultConnection", result);
        _mockSampleService.Verify(x => x.InitializeConnectionAsync("DefaultConnection"), Times.Once);
    }

    [Fact]
    public async Task Initialize_WhenServiceThrowsException_ReturnsErrorJson()
    {
        // Arrange
        var connectionName = "TestConnection";
        _mockSampleService.Setup(x => x.InitializeConnectionAsync(connectionName))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _sampleTools.Initialize(connectionName);

        // Assert
        Assert.Contains("success", result);
        Assert.Contains("false", result);
        Assert.Contains("error", result);
    }

    [Fact]
    public async Task GetSampleData_ReturnsData()
    {
        // Arrange
        var connectionName = "DefaultConnection";
        var expectedData = new[]
        {
            new { Id = 1, Name = "Sample Item 1" }
        };
        _mockSampleService.Setup(x => x.GetSampleDataAsync(connectionName))
            .ReturnsAsync(expectedData);

        // Act
        var result = await _sampleTools.GetSampleData(connectionName);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Sample Item", result);
        _mockSampleService.Verify(x => x.GetSampleDataAsync(connectionName), Times.Once);
    }

    [Fact]
    public async Task GetSampleData_WithDefaultConnection_ReturnsData()
    {
        // Arrange
        var expectedData = new[]
        {
            new { Id = 1, Name = "Sample Item 1" },
            new { Id = 2, Name = "Sample Item 2" }
        };
        _mockSampleService.Setup(x => x.GetSampleDataAsync("DefaultConnection"))
            .ReturnsAsync(expectedData);

        // Act
        var result = await _sampleTools.GetSampleData();

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Sample Item 1", result);
        Assert.Contains("Sample Item 2", result);
    }

    [Fact]
    public async Task GetSampleData_WhenServiceThrowsException_ReturnsErrorJson()
    {
        // Arrange
        var connectionName = "TestConnection";
        _mockSampleService.Setup(x => x.GetSampleDataAsync(connectionName))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _sampleTools.GetSampleData(connectionName);

        // Assert
        Assert.Contains("error", result);
        Assert.Contains("Database error", result);
    }

    [Fact]
    public async Task ExecuteOperation_WithValidOperation_ReturnsSuccess()
    {
        // Arrange
        var operation = "TestOperation";
        var parameters = "{}";
        var expectedResult = new
        {
            success = true,
            operation = operation,
            parameters = parameters
        };
        _mockSampleService.Setup(x => x.ExecuteOperationAsync(operation, parameters))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _sampleTools.ExecuteOperation(operation, parameters);

        // Assert
        Assert.Contains("success", result);
        Assert.Contains(operation, result);
        _mockSampleService.Verify(x => x.ExecuteOperationAsync(operation, parameters), Times.Once);
    }

    [Fact]
    public async Task ExecuteOperation_WithComplexParameters_ReturnsSuccess()
    {
        // Arrange
        var operation = "ComplexOperation";
        var parameters = "{\"key1\":\"value1\",\"key2\":123}";
        var expectedResult = new
        {
            success = true,
            operation = operation,
            parameters = parameters
        };
        _mockSampleService.Setup(x => x.ExecuteOperationAsync(operation, parameters))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _sampleTools.ExecuteOperation(operation, parameters);

        // Assert
        Assert.Contains("success", result);
        Assert.Contains(operation, result);
        Assert.Contains("key1", result);
        Assert.Contains("value1", result);
    }

    [Fact]
    public async Task ExecuteOperation_WhenServiceThrowsException_ReturnsErrorJson()
    {
        // Arrange
        var operation = "FailingOperation";
        var parameters = "{}";
        _mockSampleService.Setup(x => x.ExecuteOperationAsync(operation, parameters))
            .ThrowsAsync(new Exception("Operation failed"));

        // Act
        var result = await _sampleTools.ExecuteOperation(operation, parameters);

        // Assert
        Assert.Contains("success", result);
        Assert.Contains("false", result);
        Assert.Contains("error", result);
        Assert.Contains(operation, result);
    }
}
