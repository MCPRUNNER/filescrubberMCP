# GitHub Copilot Integration for Workflows

## Overview

The FileScrubberMCP now supports GitHub Copilot integration within workflows, enabling AI-powered analysis, summarization, and insights as part of multi-step data processing pipelines.

## Architecture

The GitHub Copilot integration consists of:

1. **IAIService / AIService** - Core service for AI operations
2. **AITools** - MCP tool wrapper for direct Copilot access
3. **WorkflowService Integration** - Built-in support for `#fscrub_ask_github_copilot` workflow step

## How It Works

The integration uses the Model Context Protocol (MCP) architecture:

1. **Request Phase**: When a workflow step calls `#fscrub_ask_github_copilot`, the AIService wraps the prompt in a special format
2. **Processing Phase**: The MCP client (GitHub Copilot) receives the formatted request
3. **Response Phase**: GitHub Copilot processes the prompt using its AI capabilities and returns the response
4. **Integration Phase**: The response becomes available to subsequent workflow steps

### Request Format

The AIService formats prompts as:

```
[COPILOT_REQUEST]
{your prompt here}
[/COPILOT_REQUEST]
```

This format signals to the MCP client that AI processing is required.

## Usage

### In Workflows

Add a GitHub Copilot step to your workflow JSON:

```json
{
  "Name": "AskGithubCopilot",
  "Type": "#fscrub_ask_github_copilot",
  "Parameters": {
    "prompt": "Analyze this data and provide insights: {PreviousStep.Content}"
  },
  "Enabled": true,
  "Output": {
    "Name": "Analysis",
    "Format": "Text"
  }
}
```

### Direct MCP Tool Usage

Use the `fscrub_ask_github_copilot` tool directly:

```json
{
  "tool": "fscrub_ask_github_copilot",
  "parameters": {
    "prompt": "Summarize the following employee report and identify key trends..."
  }
}
```

### Programmatic Usage

```csharp
// Inject IAIService
private readonly IAIService _aiService;

// Ask GitHub Copilot
var response = await _aiService.AskGithubCopilotAsync("Your prompt here");
```

## Complete Workflow Example

Here's the workflow from `test.json` that demonstrates the full integration:

```json
{
  "Steps": [
    {
      "Name": "GetUrlContent",
      "Type": "#fscrub_uri_get",
      "Parameters": {
        "Uri": "https://raw.githubusercontent.com/MCPRUNNER/filescrubberMCP/refs/heads/main/Examples/medium.json"
      },
      "Enabled": true,
      "Output": { "Name": "Content", "Format": "JSON" }
    },
    {
      "Name": "ProcessTemplate",
      "Type": "#fscrub_scriban_process_template",
      "Parameters": {
        "templateFilePath": "Examples/company_employee_report.sbn",
        "jsonData": "{GetUrlContent.Content}",
        "outputFilePath": "Output/company_employee_report.txt"
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
        "filePath": "Examples/medium.xml",
        "xPath": "//employee[department='Engineering']"
      },
      "Output": { "Name": "Content", "Format": "XML" },
      "Enabled": true
    },
    {
      "Name": "SaveXmlReport",
      "Type": "#fscrub_file_write",
      "Parameters": {
        "filePath": "Output\\test_workflow.txt",
        "content": "{ParseXmlExample.Content}"
      },
      "Enabled": true
    },
    {
      "Name": "AskGithubCopilot",
      "Type": "#fscrub_ask_github_copilot",
      "Parameters": {
        "prompt": "Read content and summarize:\n\n{DisplayReport.Content}\n\nAlso, analyze the following XML data and provide insights:\n\n{ParseXmlExample.Content}"
      },
      "Enabled": true
    }
  ]
}
```

## Workflow Execution Flow

1. **GetUrlContent**: Fetches JSON employee data from GitHub
2. **ProcessTemplate**: Generates a formatted report using Scriban template
3. **DisplayReport**: Reads the generated report
4. **ParseXmlExample**: Searches XML for engineering employees
5. **SaveXmlReport**: Saves the XML search results
6. **AskGithubCopilot**: Sends both the report and XML data to Copilot for AI analysis

## Use Cases

### 1. Data Summarization

```json
{
  "Name": "SummarizeReport",
  "Type": "#fscrub_ask_github_copilot",
  "Parameters": {
    "prompt": "Summarize this employee report in 3 bullet points: {ReportContent}"
  }
}
```

### 2. Insight Generation

```json
{
  "Name": "GenerateInsights",
  "Type": "#fscrub_ask_github_copilot",
  "Parameters": {
    "prompt": "Analyze this sales data and identify the top 3 trends: {SalesData}"
  }
}
```

### 3. Data Transformation Recommendations

```json
{
  "Name": "RecommendTransformations",
  "Type": "#fscrub_ask_github_copilot",
  "Parameters": {
    "prompt": "Review this data structure and suggest optimal transformations: {DataStructure}"
  }
}
```

### 4. Code Generation

```json
{
  "Name": "GenerateCode",
  "Type": "#fscrub_ask_github_copilot",
  "Parameters": {
    "prompt": "Generate a Python script to process this JSON structure: {JsonSchema}"
  }
}
```

### 5. Multi-Source Analysis

```json
{
  "Name": "CompareData",
  "Type": "#fscrub_ask_github_copilot",
  "Parameters": {
    "prompt": "Compare these two datasets and highlight differences:\n\nDataset 1:\n{Source1.Data}\n\nDataset 2:\n{Source2.Data}"
  }
}
```

## Best Practices

### 1. Clear Prompts

Write specific, actionable prompts:

- ✅ "Summarize the employee performance data and identify top performers"
- ❌ "Look at this data"

### 2. Provide Context

Include relevant data in the prompt using placeholders:

```json
"prompt": "Context: This is employee data from Q4 2024.\n\nData: {EmployeeData}\n\nTask: Identify promotion candidates."
```

### 3. Structure Complex Prompts

Use clear sections for multi-part requests:

```json
"prompt": "PART 1 - Summarize:\n{Report1}\n\nPART 2 - Compare with:\n{Report2}\n\nPART 3 - Provide recommendations"
```

### 4. Set Expectations

Guide the AI on output format:

```json
"prompt": "Analyze this data and provide:\n1. Summary (3 sentences)\n2. Key findings (bullet points)\n3. Recommendations (numbered list)\n\nData: {Data}"
```

### 5. Workflow Placement

Place Copilot steps strategically:

- **After data collection**: For initial analysis
- **After transformations**: For validation
- **At the end**: For final insights and recommendations

## Output Handling

Store Copilot responses for use in subsequent steps:

```json
{
  "Name": "GetInsights",
  "Type": "#fscrub_ask_github_copilot",
  "Parameters": {
    "prompt": "Analyze: {Data}"
  },
  "Output": {
    "Name": "Analysis",
    "Format": "Text"
  }
},
{
  "Name": "SaveInsights",
  "Type": "#fscrub_file_write",
  "Parameters": {
    "filePath": "Output/ai-insights.txt",
    "content": "{GetInsights.Analysis}"
  }
}
```

## Error Handling

The AI service includes proper error handling:

```json
{
  "success": false,
  "errorMessage": "Prompt cannot be null or empty"
}
```

Always check workflow results:

```csharp
var result = await _workflowService.ExecuteWorkflowFromFileAsync("workflow.json");
if (!result.Success)
{
    Console.WriteLine($"AI step failed: {result.ErrorMessage}");
}
```

## Performance Considerations

1. **Prompt Length**: Keep prompts focused and reasonable in size
2. **Sequential Execution**: AI steps execute in sequence with other workflow steps
3. **Response Time**: AI processing may take longer than other operations
4. **Rate Limiting**: Consider API rate limits when designing high-frequency workflows

## Security Notes

1. **Data Privacy**: Be mindful of sensitive data in prompts
2. **Prompt Injection**: Validate and sanitize user-provided prompt content
3. **Output Validation**: Always validate AI responses before using in critical operations

## Troubleshooting

### Issue: AI step returns formatted prompt instead of response

**Cause**: MCP client not processing the request format  
**Solution**: Ensure you're running within a GitHub Copilot MCP context

### Issue: Empty or null responses

**Cause**: Invalid prompt or connectivity issues  
**Solution**: Check prompt format and MCP connection status

### Issue: Workflow fails at AI step

**Cause**: Missing parameters or service configuration  
**Solution**: Verify IAIService is registered and prompt parameter is provided

## Implementation Details

### Files Created/Modified

1. **`Interfaces/IAIService.cs`** - Service interface
2. **`Services/AIService.cs`** - Service implementation
3. **`Interfaces/IAITools.cs`** - Tools interface
4. **`Tools/AITools.cs`** - MCP tools wrapper
5. **`Services/WorkflowService.cs`** - Added AI operation support
6. **`Extensions/ServiceCollectionExtensions.cs`** - DI registration

### Dependencies

- Microsoft.Extensions.Logging (for logging)
- Newtonsoft.Json (for JSON serialization)
- ModelContextProtocol.Server (for MCP integration)

## Future Enhancements

Potential improvements for the AI integration:

1. **Streaming Responses**: Support for streaming long AI responses
2. **Context Management**: Advanced context windowing for large datasets
3. **Multi-turn Conversations**: Maintain conversation state across steps
4. **Custom AI Providers**: Support for other AI services beyond Copilot
5. **Response Caching**: Cache identical prompts to improve performance
6. **Prompt Templates**: Pre-defined prompt templates for common scenarios

## Example: Complete Data Analysis Pipeline

```json
{
  "Steps": [
    {
      "Name": "FetchSalesData",
      "Type": "#fscrub_uri_get",
      "Parameters": { "Uri": "https://api.example.com/sales" },
      "Output": { "Name": "RawData", "Format": "JSON" }
    },
    {
      "Name": "ParseData",
      "Type": "#fscrub_parser_search_json",
      "Parameters": {
        "jsonFilePath": "{FetchSalesData.RawData}",
        "jsonPath": "$.sales[?(@.amount > 1000)]"
      },
      "Output": { "Name": "HighValueSales", "Format": "JSON" }
    },
    {
      "Name": "AnalyzeSales",
      "Type": "#fscrub_ask_github_copilot",
      "Parameters": {
        "prompt": "Analyze these high-value sales and provide:\n1. Top 3 products\n2. Revenue trends\n3. Customer segments\n\nData: {ParseData.HighValueSales}"
      },
      "Output": { "Name": "Analysis", "Format": "Text" }
    },
    {
      "Name": "GenerateReport",
      "Type": "#fscrub_scriban_process_template",
      "Parameters": {
        "templateFilePath": "Templates/sales-report.sbn",
        "jsonData": "{\"analysis\": \"{AnalyzeSales.Analysis}\", \"sales\": {ParseData.HighValueSales}}",
        "outputFilePath": "Reports/sales-analysis.html"
      }
    },
    {
      "Name": "SummarizeForEmail",
      "Type": "#fscrub_ask_github_copilot",
      "Parameters": {
        "prompt": "Create a brief executive summary (max 200 words) suitable for email based on this analysis: {AnalyzeSales.Analysis}"
      },
      "Output": { "Name": "EmailSummary", "Format": "Text" }
    },
    {
      "Name": "SaveSummary",
      "Type": "#fscrub_file_write",
      "Parameters": {
        "filePath": "Reports/email-summary.txt",
        "content": "{SummarizeForEmail.EmailSummary}"
      }
    }
  ]
}
```

This pipeline:

1. Fetches sales data from an API
2. Filters high-value sales using JSONPath
3. Uses Copilot to analyze the data
4. Generates a detailed HTML report
5. Creates an executive summary for email
6. Saves the summary to a file

## Conclusion

The GitHub Copilot integration enables powerful AI-driven workflows that combine data fetching, transformation, analysis, and insight generation in a seamless, automated pipeline. By leveraging the MCP protocol, workflows can now include intelligent analysis as a native step alongside traditional data operations.
