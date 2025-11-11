using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace filescrubberMCP.Services;

/// <summary>
/// Service for configuring application logging
/// </summary>
public class LoggingConfigurationService : filescrubberMCP.Interfaces.ILoggingConfigurationService
{
    /// <inheritdoc />
    public Serilog.ILogger ConfigureLogger(IConfiguration configuration, string transportType, string logsDirectory)
    {
        var loggerConfig = new LoggerConfiguration()
            .Enrich.FromLogContext();

        // In stdio mode, only write to stderr and file (not stdout)
        // Don't read from configuration to avoid duplicate console sinks
        if (transportType.Equals("Stdio", StringComparison.OrdinalIgnoreCase))
        {
            loggerConfig
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .WriteTo.Console(
                    standardErrorFromLevel: LogEventLevel.Verbose,
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");
        }
        else
        {
            // For HTTP mode, read from configuration
            loggerConfig.ReadFrom.Configuration(configuration);
        }

        // Add file logging for all modes
        loggerConfig.WriteTo.File(
            Path.Combine(logsDirectory, "templateMCP-.txt"),
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            shared: true,
            flushToDiskInterval: TimeSpan.FromSeconds(1),
            outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");

        return loggerConfig.CreateLogger();
    }
}
