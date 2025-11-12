using Microsoft.Extensions.DependencyInjection;
using filescrubberMCP.Configuration;
using filescrubberMCP.Interfaces;
using filescrubberMCP.Services;
using filescrubberMCP.Tools;

namespace filescrubberMCP.Extensions;

/// <summary>
/// Extension methods for registering MCP services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds template MCP services to the DI container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddTemplateMcp(this IServiceCollection services)
    {
        // Register core services
        services.AddSingleton<IAppConfigurationProvider, AppConfigurationProvider>();
        services.AddSingleton<IParserService, ParserService>();
        services.AddSingleton<IFileService, FileService>();
        services.AddSingleton<ITemplateService, TemplateService>();
        services.AddSingleton<IUriService, UriService>();
        services.AddSingleton<IAIService, AIService>();
        services.AddSingleton<IWorkflowService, WorkflowService>();

        // Register tools for MCP
        services.AddSingleton<SampleTools>();
        services.AddSingleton<FileTools>();
        services.AddSingleton<ParserTools>();
        services.AddSingleton<TemplateTools>();
        services.AddSingleton<UriTools>();
        services.AddSingleton<WorkflowTools>();
        services.AddSingleton<AITools>();

        return services;
    }

    /// <summary>
    /// Adds template MCP tools to the DI container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddSampleTools(this IServiceCollection services)
    {
        services.AddTransient<ISampleTools, SampleTools>();
        return services;
    }
}
