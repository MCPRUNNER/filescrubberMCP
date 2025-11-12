using filescrubberMCP.Models;

namespace filescrubberMCP.Interfaces;

/// <summary>
/// Service interface for executing workflows with sequential steps
/// </summary>
public interface IWorkflowService
{
    /// <summary>
    /// Loads a workflow definition from a JSON file
    /// </summary>
    /// <param name="workflowFilePath">Path to the workflow JSON file</param>
    /// <returns>The loaded workflow definition</returns>
    Task<WorkflowDefinition> LoadWorkflowAsync(string workflowFilePath);

    /// <summary>
    /// Executes a workflow with sequential steps
    /// </summary>
    /// <param name="workflow">The workflow definition to execute</param>
    /// <returns>The workflow execution result</returns>
    Task<WorkflowResult> ExecuteWorkflowAsync(WorkflowDefinition workflow);

    /// <summary>
    /// Loads and executes a workflow from a JSON file
    /// </summary>
    /// <param name="workflowFilePath">Path to the workflow JSON file</param>
    /// <returns>The workflow execution result</returns>
    Task<WorkflowResult> ExecuteWorkflowFromFileAsync(string workflowFilePath);
}
