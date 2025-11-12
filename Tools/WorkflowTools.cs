using System.ComponentModel;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using filescrubberMCP.Interfaces;

namespace filescrubberMCP.Tools;

/// <summary>
/// Provides MCP tools for workflow execution
/// </summary>
[McpServerToolType]
public class WorkflowTools : IWorkflowTools
{
    private readonly ILogger<WorkflowTools> _logger;
    private readonly IWorkflowService _workflowService;

    /// <summary>
    /// Initializes a new instance of the WorkflowTools class
    /// </summary>
    /// <param name="logger">Logger for the tools</param>
    /// <param name="workflowService">Service for workflow operations</param>
    public WorkflowTools(
        ILogger<WorkflowTools> logger,
        IWorkflowService workflowService)
    {
        _logger = logger;
        _workflowService = workflowService;
    }

    /// <summary>
    /// Executes a workflow from a JSON file
    /// </summary>
    [McpServerTool(Name = "fscrub_workflow_execute"), Description("Executes a workflow defined in a JSON file. The workflow contains a sequence of steps that are executed in order. Each step can reference outputs from previous steps using placeholders like {StepName.OutputName}.")]
    public async Task<string> ExecuteWorkflow(
        [Description("Path to the workflow JSON file (relative to workspace root or absolute path)")]
        string workflowFilePath)
    {
        try
        {
            _logger.LogInformation("ExecuteWorkflow tool called with file: {WorkflowFilePath}", workflowFilePath);

            var result = await _workflowService.ExecuteWorkflowFromFileAsync(workflowFilePath);

            return JsonConvert.SerializeObject(new
            {
                success = result.Success,
                errorMessage = result.ErrorMessage,
                stepResults = result.StepResults.Select(sr => new
                {
                    stepName = sr.StepName,
                    success = sr.Success,
                    errorMessage = sr.ErrorMessage,
                    executionTimeMs = sr.ExecutionTimeMs,
                    output = sr.Output
                }),
                stepOutputs = result.StepOutputs
            }, Formatting.Indented);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ExecuteWorkflow tool");
            return JsonConvert.SerializeObject(new
            {
                success = false,
                errorMessage = ex.Message,
                workflowFilePath
            }, Formatting.Indented);
        }
    }
}
