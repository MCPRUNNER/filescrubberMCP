namespace filescrubberMCP.Interfaces;

/// <summary>
/// Interface for sample MCP tools functionality
/// </summary>
public interface ISampleTools
{
    /// <summary>
    /// Initializes the connection
    /// </summary>
    /// <param name="connectionName">The name of the connection string to use</param>
    /// <returns>JSON representation of the result</returns>
    Task<string> Initialize(string connectionName = "DefaultConnection");

    /// <summary>
    /// Gets sample data
    /// </summary>
    /// <param name="connectionName">The name of the connection string to use</param>
    /// <returns>JSON representation of sample data</returns>
    Task<string> GetSampleData(string connectionName = "DefaultConnection");
}
