using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using filescrubberMCP.Services;
using filescrubberMCP.Interfaces;
using System.Threading.Tasks;
using filescrubberMCP.Models;
using System.Collections.Generic;
using System.Linq;

namespace filescrubberMCP.Tests.Services;
public class SampleServiceTests
{
    private readonly Mock<ILogger<SampleService>> _mockLogger;
    private readonly Mock<IAppConfigurationProvider> _mockConfigProvider;
    private readonly SampleService _sampleService;

    public SampleServiceTests()
    {
        _mockLogger = new Mock<ILogger<SampleService>>();
        _mockConfigProvider = new Mock<IAppConfigurationProvider>();
        _sampleService = new SampleService(_mockLogger.Object, _mockConfigProvider.Object);
    }

    [Fact]
    public async Task InitializeConnectionAsync_WithValidConnectionName_ReturnsSuccess()
    {
        // Arrange
        var connectionName = "TestConnection";
        _mockConfigProvider.Setup(x => x.GetConnectionString(connectionName))
            .Returns("Server=localhost;Database=Test;");

        // Act
        var result = await _sampleService.InitializeConnectionAsync(connectionName);
        var resultDict = result as dynamic;

        // Assert
        Assert.NotNull(result);
        Assert.True(resultDict.success);
        Assert.Equal(connectionName, resultDict.connectionName);
        Assert.Contains(connectionName, resultDict.message.ToString());
    }

    [Fact]
    public async Task InitializeConnectionAsync_WithInvalidConnectionName_ReturnsError()
    {
        // Arrange
        var connectionName = "InvalidConnection";
        _mockConfigProvider.Setup(x => x.GetConnectionString(connectionName))
            .Throws(new System.Exception("Connection not found"));

        // Act
        var result = await _sampleService.InitializeConnectionAsync(connectionName);
        var resultDict = result as dynamic;

        // Assert
        Assert.NotNull(result);
        Assert.False(resultDict.success);
        Assert.NotNull(resultDict.error);
    }

    [Fact]
    public async Task GetSampleDataAsync_ReturnsListOfSampleData()
    {
        // Arrange
        var connectionName = "DefaultConnection";

        // Act
        var result = await _sampleService.GetSampleDataAsync(connectionName);

        // Assert
        Assert.NotNull(result);
        var dataList = result as IEnumerable<SampleData>;
        Assert.NotNull(dataList);
        Assert.Equal(3, dataList.Count());
        Assert.Contains(dataList, item => item.Name == "Sample Item 1");
        Assert.Contains(dataList, item => item.Name == "Sample Item 2");
        Assert.Contains(dataList, item => item.Name == "Sample Item 3");
    }

    [Fact]
    public async Task ExecuteOperationAsync_WithValidOperation_ReturnsSuccess()
    {
        // Arrange
        var operation = "TestOperation";
        var parameters = "{\"param1\": \"value1\"}";

        // Act
        var result = await _sampleService.ExecuteOperationAsync(operation, parameters);
        var resultDict = result as dynamic;

        // Assert
        Assert.NotNull(result);
        Assert.True(resultDict.success);
        Assert.Equal(operation, resultDict.operation);
        Assert.Equal(parameters, resultDict.parameters);
        Assert.Contains(operation, resultDict.message.ToString());
    }

    [Fact]
    public async Task ExecuteOperationAsync_WithEmptyParameters_ReturnsSuccess()
    {
        // Arrange
        var operation = "SimpleOperation";
        var parameters = "{}";

        // Act
        var result = await _sampleService.ExecuteOperationAsync(operation, parameters);
        var resultDict = result as dynamic;

        // Assert
        Assert.NotNull(result);
        Assert.True(resultDict.success);
        Assert.Equal(operation, resultDict.operation);
        Assert.Equal(parameters, resultDict.parameters);
    }
}
