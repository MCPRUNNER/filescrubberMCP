namespace filescrubberMCP.Interfaces;

/// <summary>
/// Service interface for sample operations
/// </summary>
public interface ISampleService
{
    /// <summary>
    /// Initialize a connection
    /// </summary>
    /// <param name="connectionName">Name of the connection to initialize</param>
    /// <returns>Result of the initialization</returns>
    Task<object> InitializeConnectionAsync(string connectionName);

    /// <summary>
    /// Get sample data
    /// </summary>
    /// <param name="connectionName">Name of the connection to use</param>
    /// <returns>Collection of sample data</returns>
    Task<object> GetSampleDataAsync(string connectionName);

    /// <summary>
    /// Execute an operation
    /// </summary>
    /// <param name="operation">Operation to execute</param>
    /// <param name="parameters">Parameters for the operation</param>
    /// <returns>Result of the operation</returns>
    Task<object> ExecuteOperationAsync(string operation, string parameters);
}
