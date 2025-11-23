# MCP Configuration Examples

This document provides example configurations for integrating with Model Context Protocol (MCP) servers, including the FileScrubberMCP server and other MCP-compatible services.

## Overview

The MCP configuration file defines how client applications (such as AI assistants or IDEs) can connect to and interact with MCP servers. This includes specifying connection details, authentication, and environment variables.

## Configuration Structure

### Inputs

The `inputs` section defines user prompts for sensitive information like API keys:

```json
"inputs": [
  {
    "id": "mssql-server-mcp-api-key",
    "type": "promptString",
    "description": "Enter your SQL Server MCP API Key",
    "password": true
  }
]
```

- `id`: Unique identifier for the input
- `type`: Input type (e.g., "promptString")
- `description`: User-friendly description
- `password`: Whether to mask input (for security)

### Servers

The `servers` section defines available MCP servers and their connection details:

#### FileScrubberMCP (Stdio Transport)

```json
"filescrubberStdioMCP": {
  "command": "dotnet",
  "args": [
    "run",
    "--no-build",
    "--project",
    "c:\\Users\\U00001\\source\\repos\\MCP\\filescrubberMCP\\filescrubberMCP.csproj"
  ],
  "env": {
    "FILESCRUBBER_MCP_TRANSPORT": "Stdio",
    "FILESCRUBBER_FILE_DIRECTORY": "C:\\Users\\U00001\\source\\repos\\MCP\\filescrubberMCP",
    "FILESCRUBBER_MCP_LOG_DIR": "C:\\Users\\U00001\\source\\repos\\MCP\\filescrubberMCP\\Logs"
  }
}
```

This configuration runs the FileScrubberMCP server using standard I/O transport:

- `command`: Executable to run (dotnet)
- `args`: Command-line arguments
- `env`: Environment variables:
  - `FILESCRUBBER_MCP_TRANSPORT`: Set to "Stdio" for standard I/O communication
  - `FILESCRUBBER_FILE_DIRECTORY`: Root directory for file operations
  - `FILESCRUBBER_MCP_LOG_DIR`: Directory for log files

#### FileScrubberMCP (HTTP Transport)

```json
"filescrubberMCP": {
  "type": "http",
  "url": "http://localhost:5000/mcp",
  "headers": {
    "Content-Type": "application/json, text/event-stream",
    "Accept": "application/json, text/event-stream"
  }
}
```

This configuration connects to a FileScrubberMCP server running on HTTP:

- `type`: Connection type ("http")
- `url`: Server endpoint URL
- `headers`: HTTP headers for requests

#### SQL Server MCP

```json
"sql-server-mcp": {
  "url": "http://localhost:3001/mcp",
  "headers": {
    "Authorization": "Bearer ${input:mssql-server-mcp-api-key}",
    "Content-Type": "application/json"
  }
}
```

This configuration connects to a SQL Server MCP server:

- `url`: Server endpoint URL
- `headers`: HTTP headers including:
  - `Authorization`: Bearer token using the prompted API key
  - `Content-Type`: Content type for requests

## Complete Configuration Example

```json
{
  "inputs": [
    {
      "id": "mssql-server-mcp-api-key",
      "type": "promptString",
      "description": "Enter your SQL Server MCP API Key",
      "password": true
    }
  ],
  "servers": {
    "filescrubberStdioMCP": {
      "command": "dotnet",
      "args": [
        "run",
        "--no-build",
        "--project",
        "c:\\Users\\U00001\\source\\repos\\MCP\\filescrubberMCP\\filescrubberMCP.csproj"
      ],
      "env": {
        "FILESCRUBBER_MCP_TRANSPORT": "Stdio",
        "FILESCRUBBER_FILE_DIRECTORY": "C:\\Users\\U00001\\source\\repos\\MCP\\filescrubberMCP",
        "FILESCRUBBER_MCP_LOG_DIR": "C:\\Users\\U00001\\source\\repos\\MCP\\filescrubberMCP\\Logs"
      }
    },
    "filescrubberMCP": {
      "type": "http",
      "url": "http://localhost:5000/mcp",
      "headers": {
        "Content-Type": "application/json, text/event-stream",
        "Accept": "application/json, text/event-stream"
      }
    },
    "sql-server-mcp": {
      "url": "http://localhost:3001/mcp",
      "headers": {
        "Authorization": "Bearer ${input:mssql-server-mcp-api-key}",
        "Content-Type": "application/json"
      }
    }
  }
}
```

## Usage

1. **Save this configuration** to a file (e.g., `mcp.json`)
2. **Update paths** to match your local environment
3. **Configure your MCP client** to use this configuration file
4. **Start the servers** as needed (for stdio transport, the client will launch them automatically)

## Environment Variables

- `FILESCRUBBER_MCP_TRANSPORT`: "Stdio" or "Http"
- `FILESCRUBBER_FILE_DIRECTORY`: Root directory for file operations
- `FILESCRUBBER_MCP_LOG_DIR`: Log file directory

## Security Notes

- Store API keys securely using the `inputs` section
- Use HTTPS URLs in production environments
- Regularly rotate API keys and tokens
