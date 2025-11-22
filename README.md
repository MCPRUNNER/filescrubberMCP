# filescrubberMCP (MCP Server) main-20251122233044

A Model Context Protocol (MCP) server providing comprehensive file operations, parsing capabilities, and Scriban template processing. Built on .NET 10 with support for HTTP and Stdio transports.
TEST

## Notice

This project is currently in active development and may undergo significant changes. Features and APIs are subject to change, and breaking changes may occur in future releases. Use at your own discretion.

## Summary

**FileScrubberMCP** is a powerful, enterprise-ready MCP server that extends AI assistants with comprehensive file system operations, data parsing, and template rendering capabilities. It enables AI models to interact with local and remote files, query structured data across multiple formats, transform content, and generate documentation through Scriban templates.

### Key Capabilities

- **📁 File System Operations**: Read, write, and list files with full metadata including timestamps, sizes, and attributes. Supports recursive directory scanning and flexible search patterns.

- **🔍 Multi-Format Parsing**: Query and extract data from JSON, XML, YAML, CSV, and Excel files using industry-standard query languages (JSONPath for JSON/YAML/CSV/Excel, XPath for XML).

- **🔄 Data Transformation**: Transform XML documents using XSLT stylesheets with optional output to files.

- **📝 Template Processing**: Render Scriban (.sbn) templates with JSON data to generate reports, documentation, or any text-based output. Process templates to files or return as strings.

- **🌐 HTTP Client Operations**: Execute HTTP requests (GET, POST, PUT, PATCH, DELETE, HEAD, OPTIONS) with custom headers and JSON payloads.

- **⚙️ Workflow Automation**: Execute complex multi-step workflows defined in JSON files. Chain operations together, pass data between steps, and automate entire data processing pipelines.

- **🤖 AI Integration**: Built-in GitHub Copilot integration for AI-powered analysis and content generation within workflows.

- **🔌 Flexible Transport**: Supports both HTTP and Stdio transports for seamless integration with various MCP clients and AI platforms.

### Ideal Use Cases

- **Data Analysis**: Query and extract information from structured data files (JSON, XML, YAML, CSV, Excel)
- **Report Generation**: Create formatted reports from file listings, data queries, or any JSON data using Scriban templates
- **File Management**: Automate file operations, directory scanning, and content manipulation
- **Data Transformation**: Convert between formats, transform XML with XSLT, and process structured data
- **API Integration**: Make HTTP requests to REST APIs and process responses
- **Documentation**: Generate documentation from code, data, or file metadata
- **Workflow Automation**: Chain multiple operations together in declarative JSON workflows
- **AI-Powered Analysis**: Leverage GitHub Copilot for intelligent data analysis and content generation

### Architecture

Built on **.NET 10** with a clean, modular architecture:

- **Service Layer**: Business logic for file, parser, template, and URI operations
- **Tools Layer**: MCP tool implementations exposing services to AI models
- **Dependency Injection**: Full DI support for testability and maintainability
- **Structured Logging**: Comprehensive logging with Serilog for monitoring and debugging
- **Comprehensive Testing**: 84 unit tests covering services and tools with edge cases

## Features

### 🗂️ File Operations

- **Read Files** - Read file contents from any path
- **Write Files** - Write content to files with automatic directory creation
- **List Files** - Comprehensive file listing with metadata (size, dates, attributes)
- Search patterns and recursive directory scanning

### 📄 File Parsing & Querying

Support for multiple file formats with powerful query capabilities:

- **JSON** - JSONPath queries with key path preservation
- **XML** - XPath queries with namespace support
- **YAML** - JSONPath queries on YAML data
- **CSV** - JSONPath queries with header support
- **Excel (.xlsx)** - Multi-worksheet support with JSONPath queries
- **XSLT** - XML transformation capabilities (transform XML documents using XSLT stylesheets with optional file output)

### 📝 Scriban Template Processing

- **Process Templates** - Render .sbn templates with JSON data and save to file
- **Render Templates** - Render templates and return output as string
- Supports loops, conditionals, filters, and custom functions
- Example templates for file listing reports included

### 🌐 HTTP/URI Operations

Full HTTP client support for REST API integration:

- **GET, POST, PUT, PATCH, DELETE** - Standard HTTP methods
- **HEAD, OPTIONS** - HTTP metadata operations
- Custom headers and JSON payloads
- Parse and validate URIs
- Extract URI components (scheme, host, path, query, fragment)

### ⚙️ Workflow Automation

Execute complex multi-step workflows with data flow between steps:

- **Sequential Execution** - Steps run in order with context sharing
- **Data Transformation Pipelines** - Chain file operations, HTTP requests, parsing, and templates
- **Placeholder References** - Use `{StepName.OutputName}` to reference previous step outputs
- **Conditional Execution** - Enable/disable steps dynamically
- **GitHub Copilot Integration** - AI-powered analysis and content generation within workflows
- See `Documents/WORKFLOW_SERVICE_README.md` for details

## Quick Start

### Prerequisites

- .NET 10.0 SDK or later
- Windows, Linux, or macOS

> **Note**: Upgrading from .NET 9? See the [Migration Guide](Documents/MIGRATION_TO_NET10.md) for detailed upgrade instructions.

### Installation

```bash
# Clone the repository
git clone <repository-url>
cd filescrubberMCP

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run tests
dotnet test
```

### Running the Server

#### HTTP Transport (Default)

```bash
dotnet run
```

The server will start on `http://localhost:5000` (or the configured port).

#### Stdio Transport

```bash
$env:FILESCRUBBER_MCP_TRANSPORT="Stdio"
dotnet run
```

Or use the provided PowerShell scripts:

```powershell
# HTTP mode
.\Scripts\Start-Http.ps1

# Stdio mode
.\Scripts\Start-Stdio.ps1
```

## Configuration

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=app.db"
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000"],
    "AllowedMethods": ["GET", "POST", "PUT", "DELETE"],
    "AllowedHeaders": ["*"],
    "ExposedHeaders": [],
    "AllowCredentials": true
  }
}
```

### Environment Variables

- `FILESCRUBBER_MCP_TRANSPORT` - Set to `"Http"` or `"Stdio"` (default: `"Http"`)
- `FILESCRUBBER_MCP_LOG_DIR` - Custom directory for log files (default: `Logs` in application directory)
- `FILESCRUBBER_MCP_ROOT_DIR` - Root directory for file operations. When set, all relative file paths will be resolved relative to this directory. Useful for Docker deployments to set a working directory. (default: current working directory)

## MCP Tools

### File Tools

#### `fscrub_file_read`

Reads the content of a file.

**Parameters:**

- `filePath` (string) - Path to the file to read

**Example:**

```json
{
  "filePath": "C:\\Projects\\data.txt"
}
```

#### `fscrub_file_write`

Writes content to a file.

**Parameters:**

- `filePath` (string) - Path to the file to write
- `content` (string) - Content to write to the file

**Example:**

```json
{
  "filePath": "C:\\Projects\\output.txt",
  "content": "Hello, World!"
}
```

#### `fscrub_file_list`

Lists files in a directory with metadata.

**Parameters:**

- `directoryPath` (string) - Directory to search
- `searchPattern` (string, optional) - File pattern (default: "\*")
- `recursive` (bool, optional) - Search subdirectories (default: true)

**Example:**

```json
{
  "directoryPath": "C:\\Projects",
  "searchPattern": "*.cs",
  "recursive": true
}
```

### Parser Tools

#### `fscrub_parser_search_json`

Search JSON files using JSONPath.

**Parameters:**

- `jsonFilePath` (string) - Path to JSON file
- `jsonPath` (string) - JSONPath query (e.g., "$.users[*].email")
- `indented` (bool, optional) - Format output (default: true)
- `showKeyPaths` (bool, optional) - Include paths in results (default: false)

#### `fscrub_parser_search_xml`

Search XML files using XPath.

**Parameters:**

- `xmlFilePath` (string) - Path to XML file
- `xPath` (string) - XPath query (e.g., "//user/@email")
- `indented` (bool, optional) - Format output (default: true)
- `showKeyPaths` (bool, optional) - Include paths in results (default: false)

#### `fscrub_parser_search_yaml`

Search YAML files using JSONPath.

**Parameters:**

- `yamlFilePath` (string) - Path to YAML file
- `jsonPath` (string) - JSONPath query
- `indented` (bool, optional) - Format output (default: true)
- `showKeyPaths` (bool, optional) - Include paths in results (default: false)

#### `fscrub_parser_search_csv`

Search CSV files using JSONPath.

**Parameters:**

- `csvFilePath` (string) - Path to CSV file
- `jsonPath` (string) - JSONPath query
- `hasHeaderRecord` (bool, optional) - First row is header (default: true)
- `ignoreBlankLines` (bool, optional) - Ignore blank lines (default: true)

#### `fscrub_parser_search_excel`

Search Excel files using JSONPath.

**Parameters:**

- `excelFilePath` (string) - Path to Excel file (.xlsx)
- `jsonPath` (string) - JSONPath query (e.g., "$.Sheet1[*].ColumnName")

#### `fscrub_parser_transform_xml`

Transform XML using XSLT stylesheet.

**Parameters:**

- `xmlFilePath` (string) - Path to XML file
- `xsltFilePath` (string) - Path to XSLT stylesheet
- `destinationFilePath` (string, optional) - Output file path

### Scriban Template Tools

#### `fscrub_scriban_process_template`

Process a Scriban template with JSON data and save to file.

**Parameters:**

- `templateFilePath` (string) - Path to .sbn template file
- `jsonData` (string) - JSON data for template
- `outputFilePath` (string) - Output file path

**Example:**

```json
{
  "templateFilePath": "Examples/file_list_report.sbn",
  "jsonData": "{\"title\":\"Report\",\"items\":[{\"name\":\"Item1\"}]}",
  "outputFilePath": "output/report.md"
}
```

#### `fscrub_scriban_render_template`

Render a Scriban template and return the output.

**Parameters:**

- `templateFilePath` (string) - Path to .sbn template file
- `jsonData` (string) - JSON data for template

### URI Tools

#### `fscrub_uri_get`

Send a GET request to the specified URI.

**Parameters:**

- `uri` (string) - URI to send GET request to
- `headersJson` (string, optional) - JSON object of headers

#### `fscrub_uri_post`

Send a POST request to the specified URI.

**Parameters:**

- `uri` (string) - URI to send POST request to
- `jsonBody` (string, optional) - JSON body for the request
- `headersJson` (string, optional) - JSON object of headers

#### `fscrub_uri_put`

Send a PUT request to the specified URI.

**Parameters:**

- `uri` (string) - URI to send PUT request to
- `jsonBody` (string, optional) - JSON body for the request
- `headersJson` (string, optional) - JSON object of headers

#### `fscrub_uri_patch`

Send a PATCH request to the specified URI.

**Parameters:**

- `uri` (string) - URI to send PATCH request to
- `jsonBody` (string, optional) - JSON body for the request
- `headersJson` (string, optional) - JSON object of headers

#### `fscrub_uri_delete`

Send a DELETE request to the specified URI.

**Parameters:**

- `uri` (string) - URI to send DELETE request to
- `headersJson` (string, optional) - JSON object of headers

#### `fscrub_uri_head`

Send a HEAD request to the specified URI.

**Parameters:**

- `uri` (string) - URI to send HEAD request to
- `headersJson` (string, optional) - JSON object of headers

#### `fscrub_uri_options`

Send an OPTIONS request to the specified URI.

**Parameters:**

- `uri` (string) - URI to send OPTIONS request to
- `headersJson` (string, optional) - JSON object of headers

### Workflow Tools

#### `fscrub_workflow_execute`

Execute a multi-step workflow defined in a JSON file.

**Parameters:**

- `workflowFilePath` (string) - Path to the workflow JSON file

**Example:**

```json
{
  "workflowFilePath": ".fscrub/workflows/data-pipeline.json"
}
```

**Workflow Features:**

- Sequential step execution with data passing
- Reference previous step outputs using `{StepName.OutputName}`
- Support for all file, parser, template, and URI operations
- GitHub Copilot integration for AI-powered analysis
- Enable/disable individual steps
- Detailed execution metrics and error handling

See `Documents/WORKFLOW_SERVICE_README.md` for workflow definition format and examples.

### AI Integration Tools

#### `fscrub_ask_github_copilot`

Send a prompt to GitHub Copilot for AI-powered analysis or content generation.

**Parameters:**

- `prompt` (string) - The prompt to send to GitHub Copilot

**Example:**

```json
{
  "prompt": "Analyze the following employee data and provide insights:\n\n{PreviousStep.Content}"
}
```

**Note:** This tool is primarily used within workflows but can be called independently.

## Template Examples

The `Examples/` directory contains sample Scriban templates:

### File List Report Template

```scriban
# File Listing Report

**Directory:** {{ directoryPath }}
**Total Files:** {{ fileCount }}

{{ for file in files }}
- {{ file.file_name }} ({{ file.size_in_bytes }} bytes)
{{ end }}
```

### Usage Example

1. List files:

```json
fscrub_file_list("C:\\Projects", "*.cs", true)
```

2. Process template with output:

```json
fscrub_scriban_process_template(
  "Examples/file_list_report.sbn",
  <json_from_step_1>,
  "report.md"
)
```

See `Examples/SCRIBAN_TEMPLATES_README.md` for more details.

## Project Structure

```
filescrubberMCP/
├── Configuration/          # Configuration providers
├── Examples/              # Sample files and templates
│   ├── *.json            # Sample JSON/XML/YAML files
│   ├── *.sbn             # Scriban template examples
│   └── SCRIBAN_TEMPLATES_README.md
├── Extensions/            # Service collection extensions
├── Interfaces/            # Service and tool interfaces
├── Logs/                  # Application logs
├── Models/                # Data models
├── Scripts/               # PowerShell startup scripts
├── Services/              # Business logic services
│   ├── FileService.cs
│   ├── ParserService.cs
│   ├── TemplateService.cs
│   └── UriService.cs
├── Tests/                 # Unit tests
│   ├── Services/
│   └── Tools/
├── Tools/                 # MCP tool implementations
│   ├── FileTools.cs
│   ├── ParserTools.cs
│   ├── TemplateTools.cs
│   └── UriTools.cs
├── Program.cs             # Application entry point
├── appsettings.json       # Configuration
└── README.md
```

## Development

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test suite
dotnet test --filter "FullyQualifiedName~FileServiceTests"

# Run with coverage
dotnet test /p:CollectCoverage=true
```

### Adding New Tools

1. Create interface in `Interfaces/`
2. Implement service in `Services/`
3. Implement MCP tool in `Tools/`
4. Register in `Program.cs` and `ServiceCollectionExtensions.cs`
5. Add tests in `Tests/`

### Code Style

- Use async/await for I/O operations
- Follow dependency injection patterns
- Comprehensive logging with Serilog
- Structured error handling
- XML documentation comments

## Testing

The project includes comprehensive unit tests:

- **84 total tests**
- Service layer tests with mocked dependencies
- Tool layer tests with mocked services
- Edge cases and error scenarios
- Integration scenarios

See `Tests/TEMPLATE_TESTS_SUMMARY.md` for detailed test coverage.

## Dependencies

### Core

- .NET 10.0
- ModelContextProtocol.AspNetCore (0.1.0-preview.13)

### Logging

- Serilog.AspNetCore (9.0.0)

### File Processing

- ClosedXML (0.105.0) - Excel files
- CsvHelper (33.1.0) - CSV files
- YamlDotNet (16.3.0) - YAML files
- Newtonsoft.Json (13.0.3) - JSON processing

### Template Processing

- Scriban (5.10.0) - Template engine

### Testing

- xUnit (2.9.3)
- Moq (4.20.72)
- Microsoft.NET.Test.Sdk (18.0.1)

## Logging

Logs are written to the `Logs/` directory (or custom directory specified by `FILESCRUBBER_MCP_LOG_DIR` environment variable):

- **HTTP mode**: `Logs/filescrubber-mcp-http-YYYYMMDD.log`
- **Stdio mode**: `Logs/filescrubber-mcp-stdio-YYYYMMDD.log`

Log levels can be configured in `appsettings.json`.

## Error Handling

All MCP tools return structured JSON responses:

**Success:**

```json
{
  "success": true,
  "data": { ... }
}
```

**Error:**

```json
{
  "success": false,
  "error": "Error message"
}
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Ensure all tests pass
6. Submit a pull request

## License

[Your License Here]

## Support

For issues, questions, or contributions, please [open an issue](link-to-issues).

## Acknowledgments

- Built with [Model Context Protocol](https://modelcontextprotocol.io/)
- Powered by [Scriban](https://github.com/scriban/scriban) template engine
- Uses [ClosedXML](https://github.com/ClosedXML/ClosedXML) for Excel processing
