using Xunit;
using Microsoft.Extensions.Configuration;
using filescrubberMCP.Services;
using System.Collections.Generic;
using System.IO;
using System;

namespace filescrubberMCP.Tests.Services;

public class LoggingConfigurationServiceTests : IDisposable
{
    private readonly LoggingConfigurationService _loggingService;
    private readonly IConfiguration _configuration;
    private readonly string _testLogsDirectory;

    public LoggingConfigurationServiceTests()
    {
        _loggingService = new LoggingConfigurationService();

        // Create test configuration
        var configDict = new Dictionary<string, string>
        {
            {"Serilog:MinimumLevel:Default", "Information"},
            {"Serilog:MinimumLevel:Override:Microsoft", "Warning"}
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict!)
            .Build();

        // Create temp directory for test logs
        _testLogsDirectory = Path.Combine(Path.GetTempPath(), "templateMCP_Tests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testLogsDirectory);
    }

    [Fact]
    public void ConfigureLogger_WithHttpTransport_ReturnsLogger()
    {
        // Arrange
        var transportType = "Http";

        // Act
        var logger = _loggingService.ConfigureLogger(_configuration, transportType, _testLogsDirectory);

        // Assert
        Assert.NotNull(logger);
    }

    [Fact]
    public void ConfigureLogger_WithStdioTransport_ReturnsLogger()
    {
        // Arrange
        var transportType = "Stdio";

        // Act
        var logger = _loggingService.ConfigureLogger(_configuration, transportType, _testLogsDirectory);

        // Assert
        Assert.NotNull(logger);
    }

    [Fact]
    public void ConfigureLogger_WithCaseInsensitiveStdio_ReturnsLogger()
    {
        // Arrange
        var transportType = "STDIO";

        // Act
        var logger = _loggingService.ConfigureLogger(_configuration, transportType, _testLogsDirectory);

        // Assert
        Assert.NotNull(logger);
    }

    [Fact]
    public void ConfigureLogger_WithUnknownTransport_ReturnsLogger()
    {
        // Arrange
        var transportType = "Unknown";

        // Act
        var logger = _loggingService.ConfigureLogger(_configuration, transportType, _testLogsDirectory);

        // Assert
        Assert.NotNull(logger);
    }

    [Fact]
    public void ConfigureLogger_CreatesLogFile()
    {
        // Arrange
        var transportType = "Http";

        // Act
        var logger = _loggingService.ConfigureLogger(_configuration, transportType, _testLogsDirectory);
        logger.Information("Test message");

        // Give it a moment to write
        System.Threading.Thread.Sleep(100);

        // Assert
        var logFiles = Directory.GetFiles(_testLogsDirectory, "templateMCP-*.txt");
        Assert.NotEmpty(logFiles);
    }

    public void Dispose()
    {
        // Cleanup test logs directory
        try
        {
            if (Directory.Exists(_testLogsDirectory))
            {
                Directory.Delete(_testLogsDirectory, true);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}
