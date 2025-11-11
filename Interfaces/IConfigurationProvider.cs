namespace filescrubberMCP.Interfaces;

/// <summary>
/// Interface for application configuration provider
/// </summary>
public interface IAppConfigurationProvider
{
    /// <summary>
    /// Gets a connection string by name
    /// </summary>
    /// <param name="name">The name of the connection string</param>
    /// <returns>The connection string</returns>
    string GetConnectionString(string name);
}
