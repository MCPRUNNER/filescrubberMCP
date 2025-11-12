namespace filescrubberMCP.Interfaces;

/// <summary>
/// Interface for AI service operations including GitHub Copilot integration
/// </summary>
public interface IAIService
{
    /// <summary>
    /// Sends a prompt to GitHub Copilot and returns the response
    /// </summary>
    /// <param name="prompt">The prompt to send to GitHub Copilot</param>
    /// <param name="promptName">Optional custom name for the prompt file</param>
    /// <returns>The response from GitHub Copilot</returns>
    Task<string> AskGithubCopilotAsync(string prompt, string? promptName = null);
}
