# GitHub Copilot Integration - Quick Reference

## Workflow Step Format

```json
{
  "Name": "AskGithubCopilot",
  "Type": "#fscrub_ask_github_copilot",
  "Parameters": {
    "prompt": "Your prompt here with optional placeholders: {PreviousStep.Output}"
  },
  "Enabled": true,
  "Output": {
    "Name": "Response",
    "Format": "Text"
  }
}
```

## MCP Tool Usage

```json
{
  "tool": "fscrub_ask_github_copilot",
  "parameters": {
    "prompt": "Your prompt here"
  }
}
```

## C# Usage

```csharp
// Inject the service
private readonly IAIService _aiService;

// Make a request
var response = await _aiService.AskGithubCopilotAsync("Your prompt");
```

## Common Patterns

### Summarize Data

```json
"prompt": "Summarize this data in 3 bullet points: {Data}"
```

### Analyze Trends

```json
"prompt": "Identify key trends in: {TimeSeries}"
```

### Compare Sources

```json
"prompt": "Compare:\nSource A: {DataA}\nSource B: {DataB}"
```

### Generate Insights

```json
"prompt": "Analyze and provide insights: {Content}"
```

### Extract Information

```json
"prompt": "Extract key information from: {Document}"
```

## Response Handling

### Store for Later Use

```json
{
  "Name": "GetAnalysis",
  "Type": "#fscrub_ask_github_copilot",
  "Parameters": {"prompt": "Analyze: {Data}"},
  "Output": {"Name": "Analysis", "Format": "Text"}
},
{
  "Name": "SaveToFile",
  "Type": "#fscrub_file_write",
  "Parameters": {
    "filePath": "output.txt",
    "content": "{GetAnalysis.Analysis}"
  }
}
```

## Best Practices

✅ **DO**

- Write clear, specific prompts
- Provide context with data
- Structure multi-part requests
- Store outputs with meaningful names
- Place strategically in workflow

❌ **DON'T**

- Use vague prompts
- Omit necessary context
- Expect specific code execution
- Chain too many AI steps
- Ignore error handling

## Example Workflow

```json
{
  "Steps": [
    {"Name": "FetchData", "Type": "#fscrub_uri_get", ...},
    {"Name": "ParseData", "Type": "#fscrub_parser_search_json", ...},
    {
      "Name": "AnalyzeData",
      "Type": "#fscrub_ask_github_copilot",
      "Parameters": {
        "prompt": "Analyze and summarize: {ParseData.Result}"
      },
      "Output": {"Name": "Insights", "Format": "Text"}
    },
    {
      "Name": "SaveInsights",
      "Type": "#fscrub_file_write",
      "Parameters": {
        "filePath": "insights.txt",
        "content": "{AnalyzeData.Insights}"
      }
    }
  ]
}
```

## Quick Tips

1. **Prompt Length**: Keep focused (typically < 4000 chars)
2. **Context**: Include relevant data inline using placeholders
3. **Output**: Always name your outputs for reuse
4. **Error Handling**: Check workflow results
5. **Testing**: Test prompts independently first
