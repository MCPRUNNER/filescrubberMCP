using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using filescrubberMCP.Configuration;
using filescrubberMCP.Extensions;
using filescrubberMCP.Interfaces;
using filescrubberMCP.Tools;
using filescrubberMCP.Services;
using Serilog;
using Scriban.Parsing;

var builder = WebApplication.CreateBuilder(args);

// Get logs directory from environment variable or use default
var logsDirectory = Environment.GetEnvironmentVariable("FILESCRUBBER_MCP_LOG_DIR")
                    ?? Path.Combine(AppContext.BaseDirectory, "Logs");

// Ensure Logs directory exists
if (!Directory.Exists(logsDirectory))
{
    Directory.CreateDirectory(logsDirectory);
}

// Get transport type from environment variable
var transportType = Environment.GetEnvironmentVariable("FILESCRUBBER_MCP_TRANSPORT") ?? "Http";

// Note: FILESCRUBBER_MCP_ROOT_DIR is read by AppConfigurationProvider for file operations root directory

// Configure Serilog using the logging configuration service
var loggingConfigService = new LoggingConfigurationService();
Log.Logger = loggingConfigService.ConfigureLogger(builder.Configuration, transportType, logsDirectory);

builder.Host.UseSerilog();

// Add MCP services based on transport type
if (transportType.Equals("Http", StringComparison.OrdinalIgnoreCase))
{
    Log.Information("Using HTTP transport for MCP server.");
    builder.Services.AddMcpServer().WithHttpTransport()
        .WithTools<SampleTools>()
        .WithTools<FileTools>()
        .WithTools<ParserTools>()
        .WithTools<TemplateTools>()
        .WithTools<WorkflowTools>()
        .WithTools<UriTools>();
}
else if (transportType.Equals("Stdio", StringComparison.OrdinalIgnoreCase))
{
    Log.Information("Using Stdio transport for MCP server.");
    builder.Services.AddMcpServer().WithStdioServerTransport()
        .WithTools<SampleTools>()
        .WithTools<FileTools>()
        .WithTools<ParserTools>()
        .WithTools<TemplateTools>()
        .WithTools<WorkflowTools>()
        .WithTools<UriTools>();
}
else
{
    Log.Error($"Invalid TEMPLATE_MCP_TRANSPORT: {transportType}. Defaulting to HTTP transport.");
    builder.Services.AddMcpServer().WithHttpTransport()
        .WithTools<SampleTools>()
        .WithTools<FileTools>()
        .WithTools<ParserTools>()
        .WithTools<TemplateTools>()
        .WithTools<WorkflowTools>()
        .WithTools<UriTools>();
}

// Add application services
builder.Services.AddSingleton<IAppConfigurationProvider, AppConfigurationProvider>();
builder.Services.AddSingleton<ILoggingConfigurationService, LoggingConfigurationService>();
builder.Services.AddSingleton<IFileService, FileService>();
builder.Services.AddSingleton<IParserService, ParserService>();
builder.Services.AddSingleton<ITemplateService, TemplateService>();
builder.Services.AddSingleton<IWorkflowService, WorkflowService>();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IUriService, UriService>();
builder.Services.AddTransient<ISampleService, SampleService>();

// Add tools
builder.Services.AddTransient<ISampleTools, SampleTools>();
builder.Services.AddTransient<SampleTools>();
builder.Services.AddSingleton<FileTools>();
builder.Services.AddSingleton<ParserTools>();
builder.Services.AddSingleton<TemplateTools>();
builder.Services.AddSingleton<WorkflowTools>();
builder.Services.AddSingleton<UriTools>();

// Add logging
builder.Services.AddLogging();

// Add controllers for JSON-RPC endpoints
builder.Services.AddControllers();

// Add CORS support
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", corsBuilder =>
    {
        var corsConfig = builder.Configuration.GetSection("Cors");
        var origins = corsConfig.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
        var methods = corsConfig.GetSection("AllowedMethods").Get<string[]>() ?? Array.Empty<string>();
        var headers = corsConfig.GetSection("AllowedHeaders").Get<string[]>() ?? Array.Empty<string>();
        var exposedHeaders = corsConfig.GetSection("ExposedHeaders").Get<string[]>() ?? Array.Empty<string>();
        var allowCredentials = corsConfig.GetValue<bool>("AllowCredentials");

        corsBuilder
            .WithOrigins(origins)
            .WithMethods(methods)
            .WithHeaders(headers)
            .WithExposedHeaders(exposedHeaders);

        if (allowCredentials)
        {
            corsBuilder.AllowCredentials();
        }
    });
});

var app = builder.Build();

// Global error handling - MUST be first to catch all exceptions
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (OperationCanceledException)
    {
        // Client disconnected or request was cancelled - this is normal, just log it
        Log.Debug("Request cancelled for path {Path}", context.Request.Path);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Unhandled exception occurred");

        // Only modify response if it hasn't started yet
        if (!context.Response.HasStarted)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"error\": \"An unexpected error occurred. Please try again later.\"}");
        }
    }
});

// Configure the HTTP request pipeline
app.UseCors("CorsPolicy");

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();

// Add middleware to handle MCP content types
app.Use(async (context, next) =>
{
    // Accept both application/json and text/event-stream for MCP
    if (context.Request.Path.StartsWithSegments("/mcp"))
    {
        // Check if client accepts server-sent events
        var acceptHeader = context.Request.Headers["Accept"].ToString();
        if (acceptHeader.Contains("text/event-stream"))
        {
            context.Response.Headers["Content-Type"] = "text/event-stream";
        }
    }
    await next();
});

// Run the application
app.Lifetime.ApplicationStarted.Register(() => Log.Information("Template MCP Server started"));

if (transportType.Equals("Stdio", StringComparison.OrdinalIgnoreCase))
{
    // In stdio mode, we only want to use the MCP server transport
    var mcpServer = app.Services.GetRequiredService<ModelContextProtocol.Server.IMcpServer>();
    await mcpServer.RunAsync(CancellationToken.None);
}
else
{
    // For HTTP mode, start the web server
    app.MapMcp("/mcp");
    app.Run();
}

// Global logger factory for static classes
public static class LoggerFactory
{
    public static ILoggerFactory Create(Action<ILoggingBuilder> configure)
    {
        return Microsoft.Extensions.Logging.LoggerFactory.Create(configure);
    }
}
