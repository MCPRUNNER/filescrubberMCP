# PowerShell script to start the MCP server with HTTP transport

Write-Host "Starting Template MCP Server with HTTP transport..." -ForegroundColor Green

# Set the transport type
$env:TEMPLATE_MCP_TRANSPORT = "Http"

# Check if we're in a publish folder or development environment
if (Test-Path "./filescrubberMCP.dll") {
    # Published version
    Write-Host "Starting published version..." -ForegroundColor Cyan
    dotnet filescrubberMCP.dll
}
else {
    # Development version
    Write-Host "Starting development version..." -ForegroundColor Cyan
    dotnet run
}
