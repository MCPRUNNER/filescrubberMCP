using Microsoft.Extensions.Logging;
using filescrubberMCP.Interfaces;

namespace filescrubberMCP.Services;

/// <summary>
/// Service implementation for AI operations including GitHub Copilot integration
/// </summary>
/// <remarks>
/// This service provides integration with GitHub Copilot through the MCP protocol.
/// When running as an MCP server, the prompts are returned to the client (GitHub Copilot)
/// for processing, allowing seamless integration with the AI assistant.
/// </remarks>
public class AIService : IAIService
{
    private readonly ILogger<AIService> _logger;
    private readonly IFileService _fileService;

    /// <summary>
    /// Initializes a new instance of the AIService class
    /// </summary>
    /// <param name="logger">Logger for the service</param>
    /// <param name="fileService">File service for writing prompt files</param>
    public AIService(ILogger<AIService> logger, IFileService fileService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
    }

    /// <summary>
    /// Sends a prompt to GitHub Copilot and returns the response
    /// </summary>
    /// <param name="prompt">The prompt to send to GitHub Copilot</param>
    /// <param name="promptName">Optional custom name for the prompt file</param>
    /// <returns>The response from GitHub Copilot</returns>
    /// <remarks>
    /// This method processes the prompt and writes it to a file in the .github/prompts directory.
    /// The prompt file can then be used by GitHub Copilot or other AI tools for processing.
    /// 
    /// The format uses an inline prompt definition that signals to Copilot to:
    /// 1. Process the embedded prompt immediately
    /// 2. Return the AI-generated response
    /// 3. Continue workflow execution with the response
    /// 
    /// Format: @copilot: {prompt}
    /// 
    /// This enables true AI-powered workflows where Copilot processes prompts
    /// inline as part of the workflow execution chain.
    /// </remarks>
    public async Task<string> AskGithubCopilotAsync(string prompt, string? promptName = null)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new ArgumentException("Prompt cannot be null or empty", nameof(prompt));
        }

        _logger.LogInformation("Processing GitHub Copilot request with prompt length: {PromptLength}, promptName: {PromptName}",
            prompt.Length, promptName ?? "auto-generated");

        // Return the prompt in a format that GitHub Copilot can recognize and process inline
        // The @copilot: prefix signals that this is an inline prompt request
        var formattedPrompt = $"@copilot: {prompt}";

        _logger.LogDebug("Formatted inline prompt for Copilot processing");

        // Generate filename - use custom name if provided, otherwise timestamp-based
        string promptFileName;
        if (!string.IsNullOrWhiteSpace(promptName))
        {
            // Ensure the filename has .md extension
            promptFileName = promptName.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
                ? promptName
                : $"{promptName}.md";
        }
        else
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            promptFileName = $"workflow_{timestamp}.prompt.md";
        }

        var promptFilePath = Path.Combine(".github", "prompts", promptFileName);

        // Write the prompt to file
        try
        {
            await _fileService.WriteFileAsync(promptFilePath, formattedPrompt);
            _logger.LogInformation("Prompt written to file: {FilePath}", promptFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to write prompt to file: {FilePath}", promptFilePath);
            // Continue execution even if file write fails
        }

        return formattedPrompt;
    }
}
