# Workflow Service

The Workflow Service allows you to execute a sequence of operations defined in a JSON file. Each step in the workflow can reference outputs from previous steps, enabling powerful data transformation pipelines.

## Overview

The workflow system executes steps sequentially, passing data between steps using a context mechanism. Each step can:

- Execute different types of operations (URI requests, file operations, template processing, data parsing)
- Reference outputs from previous steps using placeholders
- Store its output for use by subsequent steps
- Be enabled or disabled individually

## Workflow Definition Format

A workflow is defined in a JSON file with the following structure:

```json
{
  "Steps": [
    {
      "Name": "StepName",
      "Type": "#operation_type",
      "Parameters": {
        "param1": "value1",
        "param2": "{PreviousStep.OutputName}"
      },
      "Enabled": true,
      "Output": {
        "Name": "OutputName",
        "Format": "JSON"
      }
    }
  ]
}
```

### Step Properties

- **Name**: Unique identifier for the step (used for referencing outputs)
- **Type**: The operation to perform (see Supported Operations below)
- **Parameters**: Dictionary of parameters required by the operation
- **Enabled**: Boolean flag to enable/disable the step (default: true)
- **Output**: Optional output configuration
  - **Name**: Name to store the output under
  - **Format**: Output format hint (e.g., "JSON", "Text")

### Placeholder Syntax

Reference outputs from previous steps using curly braces:

- `{StepName.OutputName}` - References a named output from a step
- `{StepName}` - References the entire output of a step

## Supported Operations

### URI Operations

- `#fscrub_uri_get` - GET request
- `#fscrub_uri_post` - POST request
- `#fscrub_uri_put` - PUT request
- `#fscrub_uri_delete` - DELETE request
- `#fscrub_uri_patch` - PATCH request
- `#fscrub_uri_head` - HEAD request
- `#fscrub_uri_options` - OPTIONS request

### File Operations

- `#fscrub_file_read` - Read file content
- `#fscrub_file_write` - Write content to file
- `#fscrub_file_list` - List files in directory

### Template Operations

- `#fscrub_scriban_process_template` - Process Scriban template and save to file
- `#fscrub_scriban_render_template` - Render Scriban template to string

### Parser Operations

- `#fscrub_parser_search_json` - Search JSON using JSONPath
- `#fscrub_parser_search_xml` - Search XML using XPath
- `#fscrub_parser_search_yaml` - Search YAML using JSONPath
- `#fscrub_parser_search_csv` - Search CSV using JSONPath
- `#fscrub_parser_search_excel` - Search Excel using JSONPath
- `#fscrub_parser_transform_xml` - Transform XML using XSLT

## Example Workflow

Here's a complete example that:

1. Fetches JSON data from a URL
2. Processes it with a Scriban template
3. Reads the generated report
4. Searches XML data
5. Saves the search results

```json
{
  "Steps": [
    {
      "Name": "GetUrlContent",
      "Type": "#fscrub_uri_get",
      "Parameters": {
        "Uri": "https://api.example.com/data.json"
      },
      "Enabled": true,
      "Output": {
        "Name": "Content",
        "Format": "JSON"
      }
    },
    {
      "Name": "ProcessTemplate",
      "Type": "#fscrub_scriban_process_template",
      "Parameters": {
        "templateFilePath": "Templates/report.sbn",
        "jsonData": "{GetUrlContent.Content}",
        "outputFilePath": "Output/report.txt"
      },
      "Enabled": true
    },
    {
      "Name": "DisplayReport",
      "Type": "#fscrub_file_read",
      "Parameters": {
        "filePath": "{ProcessTemplate.outputFilePath}"
      },
      "Enabled": true
    },
    {
      "Name": "ParseXmlExample",
      "Type": "#fscrub_parser_search_xml",
      "Parameters": {
        "filePath": "Data/employees.xml",
        "xPath": "//employee[department='Engineering']"
      },
      "Enabled": true
    },
    {
      "Name": "SaveXmlReport",
      "Type": "#fscrub_file_write",
      "Parameters": {
        "filePath": "Output/engineers.txt",
        "content": "{ParseXmlExample.Result}"
      },
      "Enabled": true
    }
  ]
}
```

## Usage

### Via MCP Tool

Use the `fscrub_workflow_execute` MCP tool:

```json
{
  "tool": "fscrub_workflow_execute",
  "parameters": {
    "workflowFilePath": ".fscrub/workflows/my-workflow.json"
  }
}
```

### Via Code

```csharp
// Inject IWorkflowService
private readonly IWorkflowService _workflowService;

// Execute workflow from file
var result = await _workflowService.ExecuteWorkflowFromFileAsync("path/to/workflow.json");

if (result.Success)
{
    Console.WriteLine("Workflow completed successfully!");
    foreach (var stepResult in result.StepResults)
    {
        Console.WriteLine($"Step {stepResult.StepName}: {stepResult.ExecutionTimeMs}ms");
    }
}
else
{
    Console.WriteLine($"Workflow failed: {result.ErrorMessage}");
}
```

### Programmatic Workflow Creation

```csharp
var workflow = new WorkflowDefinition
{
    Steps = new List<WorkflowStep>
    {
        new WorkflowStep
        {
            Name = "FetchData",
            Type = "#fscrub_uri_get",
            Parameters = new Dictionary<string, object>
            {
                { "Uri", "https://api.example.com/data" }
            },
            Enabled = true,
            Output = new WorkflowStepOutput
            {
                Name = "Data",
                Format = "JSON"
            }
        },
        new WorkflowStep
        {
            Name = "SaveData",
            Type = "#fscrub_file_write",
            Parameters = new Dictionary<string, object>
            {
                { "filePath", "output.json" },
                { "content", "{FetchData.Data}" }
            },
            Enabled = true
        }
    }
};

var result = await _workflowService.ExecuteWorkflowAsync(workflow);
```

## Workflow Result

The workflow execution returns a `WorkflowResult` object with:

```csharp
public class WorkflowResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> StepOutputs { get; set; }
    public List<WorkflowStepResult> StepResults { get; set; }
}

public class WorkflowStepResult
{
    public string StepName { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public object? Output { get; set; }
    public long ExecutionTimeMs { get; set; }
}
```

## Error Handling

- If a step fails, the workflow stops immediately
- The error is captured in the `WorkflowResult.ErrorMessage`
- Partial results from completed steps are available in `StepResults`
- Each step's execution time is tracked for performance analysis

## Best Practices

1. **Use Descriptive Names**: Give steps clear, descriptive names
2. **Enable/Disable Steps**: Use the `Enabled` flag for debugging
3. **Name Outputs**: Always name outputs that will be referenced later
4. **Error Recovery**: Check workflow results and handle errors appropriately
5. **Path Resolution**: Use relative paths for portability
6. **Parameter Validation**: Ensure all required parameters are provided
7. **Context References**: Verify step dependencies are correct

## Limitations

- Steps execute sequentially (no parallel execution)
- Workflow stops on first error
- No conditional execution or loops
- Context values are stored as strings
- Parameter names are case-sensitive
