FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj file and restore dependencies
COPY ["filescrubberMCP.csproj", "./"]
RUN dotnet restore

# Copy the rest of the source code
COPY . .

# Build the application
RUN dotnet build "filescrubberMCP.csproj" -c Release -o /app/build
RUN dotnet publish "filescrubberMCP.csproj" -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Create directories for data and logs
RUN mkdir -p /app/Data /app/Logs

# Copy the published application
COPY --from=build /app/publish .

# Set environment variables with defaults (these should be overridden at runtime)
ENV TEMPLATE_MCP_TRANSPORT="Http"
ENV ASPNETCORE_URLS="http://+:3001"
ENV FILESCRUBBER_MCP_ROOT_DIR="/app/Data"
ENV FILESCRUBBER_MCP_LOG_DIR="/app/Logs"

# Expose the MCP server port
EXPOSE 3001

# Set the entry point
ENTRYPOINT ["dotnet", "filescrubberMCP.dll"]
