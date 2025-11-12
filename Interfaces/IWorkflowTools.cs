namespace filescrubberMCP.Interfaces;

/// <summary>
/// Tool interface for workflow operations
/// </summary>
public interface IWorkflowTools
{
    /// <summary>
    /// Executes a workflow from a JSON file
    /// </summary>
    Task<string> ExecuteWorkflow(string workflowFilePath);
}
