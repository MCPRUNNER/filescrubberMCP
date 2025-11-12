namespace filescrubberMCP.Models;

/// <summary>
/// Represents a single step in a workflow
/// </summary>
public class WorkflowStep
{
    /// <summary>
    /// The name of the step for identification and output reference
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The type of operation to perform (e.g., #fscrub_uri_get, #fscrub_file_read)
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Parameters for the step operation
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();

    /// <summary>
    /// Whether this step is enabled for execution
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Output configuration for the step
    /// </summary>
    public WorkflowStepOutput? Output
    {
        get; set;
    }
}

/// <summary>
/// Represents the output configuration for a workflow step
/// </summary>
public class WorkflowStepOutput
{
    /// <summary>
    /// The name of the output variable
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The format of the output (e.g., JSON, Text)
    /// </summary>
    public string? Format
    {
        get; set;
    }
}
