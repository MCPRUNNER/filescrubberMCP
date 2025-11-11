# FileScrubber MCP Server

A Model Context Protocol (MCP) server providing comprehensive file operations, parsing capabilities, and Scriban template processing. Built on .NET 9 with support for HTTP and Stdio transports.

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
- **XSLT** - XML transformation capabilities

### 📝 Scriban Template Processing

- **Process Templates** - Render .sbn templates with JSON data and save to file
- **Render Templates** - Render templates and return output as string
- Supports loops, conditionals, filters, and custom functions
- Example templates for file listing reports included

### 🔗 URI Operations

- Parse and validate URIs
- Extract components (scheme, host, path, query, fragment)
- Build URIs from components

## Quick Start

### Prerequisites

- .NET 9.0 SDK or later
- Windows, Linux, or macOS

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

#### `fscrub_parse_json`

Search JSON files using JSONPath.

**Parameters:**

- `jsonFilePath` (string) - Path to JSON file
- `jsonPath` (string) - JSONPath query (e.g., "$.users[*].email")
- `indented` (bool, optional) - Format output (default: true)
- `showKeyPaths` (bool, optional) - Include paths in results (default: false)

#### `fscrub_parse_xml`

Search XML files using XPath.

**Parameters:**

- `xmlFilePath` (string) - Path to XML file
- `xPath` (string) - XPath query (e.g., "//user/@email")
- `indented` (bool, optional) - Format output (default: true)
- `showKeyPaths` (bool, optional) - Include paths in results (default: false)

#### `fscrub_parse_yaml`

Search YAML files using JSONPath.

**Parameters:**

- `yamlFilePath` (string) - Path to YAML file
- `jsonPath` (string) - JSONPath query
- `indented` (bool, optional) - Format output (default: true)
- `showKeyPaths` (bool, optional) - Include paths in results (default: false)

#### `fscrub_parse_csv`

Search CSV files using JSONPath.

**Parameters:**

- `csvFilePath` (string) - Path to CSV file
- `jsonPath` (string) - JSONPath query
- `hasHeaderRecord` (bool, optional) - First row is header (default: true)
- `ignoreBlankLines` (bool, optional) - Ignore blank lines (default: true)

#### `fscrub_parse_excel`

Search Excel files using JSONPath.

**Parameters:**

- `excelFilePath` (string) - Path to Excel file (.xlsx)
- `jsonPath` (string) - JSONPath query (e.g., "$.Sheet1[*].ColumnName")

#### `fscrub_transform_xml_xslt`

Transform XML using XSLT stylesheet.

**Parameters:**

- `xmlFilePath` (string) - Path to XML file
- `xsltFilePath` (string) - Path to XSLT stylesheet
- `destinationFilePath` (string, optional) - Output file path

### Scriban Template Tools

#### `scriban_process_template`

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

#### `scriban_render_template`

Render a Scriban template and return the output.

**Parameters:**

- `templateFilePath` (string) - Path to .sbn template file
- `jsonData` (string) - JSON data for template

### URI Tools

#### `fscrub_parse_uri`

Parse and validate URIs.

**Parameters:**

- `uri` (string) - URI to parse

#### `fscrub_build_uri`

Build a URI from components.

**Parameters:**

- `scheme` (string) - URI scheme (e.g., "https")
- `host` (string) - Host name
- `port` (int, optional) - Port number
- `path` (string, optional) - Path
- `query` (string, optional) - Query string
- `fragment` (string, optional) - Fragment

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
scriban_process_template(
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

- .NET 9.0
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
