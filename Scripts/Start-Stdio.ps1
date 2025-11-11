# PowerShell script to start the MCP server with Stdio transport

Write-Host "Starting Template MCP Server with Stdio transport..." -ForegroundColor Green

# Set the transport type
$env:TEMPLATE_MCP_TRANSPORT = "Stdio"

# Check if we're in a publish folder or development environment
if (Test-Path "./filescribberMCP.dll") {
    # Published version
    Write-Host "Starting published version..." -ForegroundColor Cyan
    dotnet filescribberMCP.dll
}
else {
    # Development version
    Write-Host "Starting development version..." -ForegroundColor Cyan
    dotnet run
}
