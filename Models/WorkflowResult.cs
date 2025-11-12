namespace filescrubberMCP.Models;

/// <summary>
/// Represents the result of a workflow execution
/// </summary>
public class WorkflowResult
{
    /// <summary>
    /// Whether the workflow execution was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if the workflow failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// The outputs from each step, keyed by step name
    /// </summary>
    public Dictionary<string, object> StepOutputs { get; set; } = new();

    /// <summary>
    /// The results from each step execution
    /// </summary>
    public List<WorkflowStepResult> StepResults { get; set; } = new();
}

/// <summary>
/// Represents the result of a single workflow step execution
/// </summary>
public class WorkflowStepResult
{
    /// <summary>
    /// The name of the step
    /// </summary>
    public string StepName { get; set; } = string.Empty;

    /// <summary>
    /// Whether the step execution was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if the step failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// The output from the step execution
    /// </summary>
    public object? Output { get; set; }

    /// <summary>
    /// The execution time in milliseconds
    /// </summary>
    public long ExecutionTimeMs { get; set; }
}
