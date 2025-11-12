using System.ComponentModel;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using filescrubberMCP.Interfaces;

namespace filescrubberMCP.Tools;

/// <summary>
/// Provides MCP tools for AI operations including GitHub Copilot integration
/// </summary>
[McpServerToolType]
public class AITools : IAITools
{
    private readonly ILogger<AITools> _logger;
    private readonly IAIService _aiService;

    /// <summary>
    /// Initializes a new instance of the AITools class
    /// </summary>
    /// <param name="logger">Logger for the tools</param>
    /// <param name="aiService">Service for AI operations</param>
    public AITools(
        ILogger<AITools> logger,
        IAIService aiService)
    {
        _logger = logger;
        _aiService = aiService;
    }

    /// <summary>
    /// Sends a prompt to GitHub Copilot and returns the response
    /// </summary>
    [McpServerTool(Name = "fscrub_ask_github_copilot"), Description("Sends a prompt to GitHub Copilot for AI-powered analysis, summarization, or insights. Use this to get intelligent responses about data, generate reports, or analyze content.")]
    public async Task<string> AskGithubCopilot(
        [Description("The prompt to send to GitHub Copilot. This can include questions, requests for analysis, or instructions for content generation.")]
        string prompt,
        [Description("Optional custom name for the prompt file. If not provided, a timestamp-based name will be generated.")]
        string? promptName = null)
    {
        try
        {
            _logger.LogInformation("AskGithubCopilot tool called with prompt length: {PromptLength}, promptName: {PromptName}",
                prompt?.Length ?? 0, promptName ?? "auto-generated");

            if (string.IsNullOrWhiteSpace(prompt))
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    errorMessage = "Prompt cannot be null or empty"
                }, Formatting.Indented);
            }

            var result = await _aiService.AskGithubCopilotAsync(prompt, promptName);

            return JsonConvert.SerializeObject(new
            {
                success = true,
                response = result,
                promptLength = prompt.Length,
                promptName = promptName ?? "auto-generated"
            }, Formatting.Indented);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AskGithubCopilot tool");
            return JsonConvert.SerializeObject(new
            {
                success = false,
                errorMessage = ex.Message
            }, Formatting.Indented);
        }
    }
}
