namespace filescrubberMCP.Models;

/// <summary>
/// Represents a workflow definition containing a sequence of steps
/// </summary>
public class WorkflowDefinition
{
    /// <summary>
    /// The list of steps to execute in sequence
    /// </summary>
    public List<WorkflowStep> Steps { get; set; } = new();
}
