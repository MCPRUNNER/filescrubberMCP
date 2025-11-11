using Serilog;

namespace filescrubberMCP.Interfaces;

/// <summary>
/// Service interface for configuring logging
/// </summary>
public interface ILoggingConfigurationService
{
    /// <summary>
    /// Configures Serilog based on the transport type
    /// </summary>
    /// <param name="configuration">Application configuration</param>
    /// <param name="transportType">Transport type (Http or Stdio)</param>
    /// <param name="logsDirectory">Directory for log files</param>
    /// <returns>Configured logger</returns>
    Serilog.ILogger ConfigureLogger(Microsoft.Extensions.Configuration.IConfiguration configuration, string transportType, string logsDirectory);
}
