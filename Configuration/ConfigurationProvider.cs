using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using filescrubberMCP.Interfaces;

namespace filescrubberMCP.Configuration;

/// <summary>
/// Provides access to configuration settings
/// </summary>
public class AppConfigurationProvider : IAppConfigurationProvider
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AppConfigurationProvider> _logger;

    /// <summary>
    /// Initializes a new instance of the AppConfigurationProvider
    /// </summary>
    /// <param name="configuration">The application configuration</param>
    /// <param name="logger">The logger</param>
    public AppConfigurationProvider(
        IConfiguration configuration,
        ILogger<AppConfigurationProvider> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Gets a connection string by name
    /// </summary>
    /// <param name="name">The name of the connection string</param>
    /// <returns>The connection string</returns>
    public string GetConnectionString(string name)
    {
        var connectionString = _configuration.GetConnectionString(name);

        if (string.IsNullOrEmpty(connectionString))
        {
            _logger.LogWarning("Connection string '{ConnectionName}' was not found", name);
            throw new InvalidOperationException($"Connection string '{name}' was not found in configuration");
        }

        return connectionString;
    }
}
