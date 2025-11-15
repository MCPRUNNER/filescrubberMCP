# Migration Guide: Upgrading to .NET 10

## Overview

This guide provides step-by-step instructions for upgrading FileScrubberMCP from .NET 9 to .NET 10. The project has been successfully migrated to take advantage of the latest .NET features and performance improvements.

## Prerequisites

Before upgrading, ensure you have:

- **.NET 10.0 SDK** installed ([Download here](https://dotnet.microsoft.com/download/dotnet/10.0))
- **Visual Studio 2022** (version 17.12 or later) or **VS Code** with latest C# extension
- **Git** for version control (recommended)

### Verify SDK Installation

Check your installed .NET SDK versions:

```powershell
dotnet --list-sdks
```

You should see `10.0.xxx` in the list.

## What Changed

### Project File Updates

The target framework has been updated from `net9.0` to `net10.0`:

```xml
<TargetFramework>net10.0</TargetFramework>
```

### Package Updates

The following packages were updated to .NET 10 compatible versions:

- **Microsoft.Data.Sqlite**: `9.0.0` → `10.0.0`
- **Microsoft.Extensions.Logging**: `9.0.0` → `10.0.0`

All other packages remain compatible with .NET 10.

### Docker Updates

The Dockerfile now uses .NET 10 base images:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
```

## Migration Steps

### Step 1: Backup Your Project

Before making any changes, create a backup:

```powershell
# Create a git branch for the migration
git checkout -b net10_migration

# Or create a backup copy
Copy-Item -Path "." -Destination "../filescrubberMCP_backup" -Recurse
```

### Step 2: Install .NET 10 SDK

Download and install the .NET 10 SDK:

1. Visit [https://dotnet.microsoft.com/download/dotnet/10.0](https://dotnet.microsoft.com/download/dotnet/10.0)
2. Download the SDK for your platform (Windows, macOS, or Linux)
3. Run the installer
4. Verify installation:

```powershell
dotnet --version
# Should show 10.0.xxx
```

### Step 3: Update Project File

Update `filescrubberMCP.csproj`:

```xml
<TargetFramework>net10.0</TargetFramework>
```

### Step 4: Update Package References

Update the Microsoft packages to .NET 10 versions:

```xml
<PackageReference Include="Microsoft.Data.Sqlite" Version="10.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging" Version="10.0.0" />
```

### Step 5: Clean and Restore

Clean the solution and restore packages:

```powershell
# Clean the build artifacts
dotnet clean

# Remove obj and bin directories
Remove-Item -Path "obj","bin" -Recurse -Force -ErrorAction SilentlyContinue

# Restore NuGet packages
dotnet restore
```

### Step 6: Build the Project

Build the project to verify everything compiles:

```powershell
dotnet build
```

Expected output:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### Step 7: Run Tests

Verify all tests pass:

```powershell
dotnet test
```

Expected output:

```
Passed!  - Failed:     0, Passed:    84, Skipped:     0, Total:    84
```

### Step 8: Update Docker (if using containers)

Update your Dockerfile to use .NET 10 images:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
```

Rebuild the Docker image:

```powershell
docker build -t filescrubber-mcp:net10 .
```

### Step 9: Test the Application

Run the application and verify functionality:

```powershell
# Test HTTP mode
.\Scripts\Start-Http.ps1

# Test Stdio mode
.\Scripts\Start-Stdio.ps1
```

### Step 10: Update Documentation

Update any project-specific documentation that references .NET 9 to .NET 10.

## Compatibility Notes

### Breaking Changes

.NET 10 introduces minimal breaking changes that affect FileScrubberMCP:

- ✅ All existing functionality remains compatible
- ✅ No API changes required in service or tool implementations
- ✅ All NuGet packages are compatible with .NET 10

### Performance Improvements

.NET 10 includes several performance enhancements:

- **Faster JIT compilation** - Improved startup time
- **Enhanced GC** - Better memory management
- **HTTP/3 improvements** - Better network performance
- **LINQ optimizations** - Faster query execution

### New Features Available

With .NET 10, you can leverage:

- **Primary constructors** for more classes (already using C# 12 features)
- **Collection expressions** for cleaner initialization
- **Improved pattern matching** for more expressive code
- **Enhanced async/await** performance

## Troubleshooting

### Issue: SDK Not Found

**Error:**

```
The current .NET SDK does not support targeting .NET 10.0
```

**Solution:**

Ensure you have installed .NET 10 SDK, not just the runtime:

```powershell
dotnet --list-sdks
```

If .NET 10 SDK is missing, download from [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/10.0).

### Issue: Package Restore Fails

**Error:**

```
Package 'Microsoft.Data.Sqlite 10.0.0' is not compatible with 'net9.0'
```

**Solution:**

Ensure your project file has been updated to target `net10.0`:

```xml
<TargetFramework>net10.0</TargetFramework>
```

Then run:

```powershell
dotnet clean
dotnet restore
```

### Issue: Docker Build Fails

**Error:**

```
Unable to find image 'mcr.microsoft.com/dotnet/sdk:10.0'
```

**Solution:**

Pull the latest .NET 10 Docker images:

```powershell
docker pull mcr.microsoft.com/dotnet/sdk:10.0
docker pull mcr.microsoft.com/dotnet/aspnet:10.0
```

### Issue: Tests Fail After Migration

**Solution:**

1. Clean test artifacts:

```powershell
dotnet clean
Remove-Item -Path "TestResults" -Recurse -Force -ErrorAction SilentlyContinue
```

2. Rebuild and run tests:

```powershell
dotnet build
dotnet test
```

## Rollback Instructions

If you need to revert to .NET 9:

### Option 1: Using Git

```powershell
git checkout main
git branch -D net10_migration
```

### Option 2: Manual Rollback

1. Update `filescrubberMCP.csproj`:

```xml
<TargetFramework>net9.0</TargetFramework>
```

2. Downgrade packages:

```xml
<PackageReference Include="Microsoft.Data.Sqlite" Version="9.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging" Version="9.0.0" />
```

3. Clean and restore:

```powershell
dotnet clean
dotnet restore
dotnet build
```

## CI/CD Considerations

If you're using continuous integration:

### GitHub Actions

Update your workflow to use .NET 10:

```yaml
- name: Setup .NET
  uses: actions/setup-dotnet@v3
  with:
    dotnet-version: "10.0.x"
```

### Azure DevOps

Update your pipeline YAML:

```yaml
- task: UseDotNet@2
  inputs:
    packageType: "sdk"
    version: "10.0.x"
```

### Jenkins

Update your Jenkinsfile:

```groovy
stage('Build') {
    steps {
        sh 'dotnet build --framework net10.0'
    }
}
```

## Post-Migration Checklist

After completing the migration, verify:

- ✅ Project builds without errors
- ✅ All 84 unit tests pass
- ✅ Application runs in HTTP mode
- ✅ Application runs in Stdio mode
- ✅ Docker image builds successfully (if applicable)
- ✅ Docker container runs correctly (if applicable)
- ✅ All MCP tools function as expected
- ✅ Workflow execution works correctly
- ✅ GitHub Copilot integration operates normally
- ✅ File operations complete successfully
- ✅ Parser tools work with all formats (JSON, XML, YAML, CSV, Excel)
- ✅ Template rendering produces correct output
- ✅ HTTP/URI operations function properly
- ✅ Logging outputs to correct files

## Benefits of .NET 10

After upgrading, you'll benefit from:

### Performance

- **20-30% faster startup time** for typical workloads
- **Reduced memory allocation** in common scenarios
- **Improved HTTP performance** for URI operations
- **Faster JSON serialization** for parser operations

### Developer Experience

- **Better error messages** for easier debugging
- **Improved IntelliSense** in Visual Studio and VS Code
- **Enhanced debugging tools** for complex scenarios
- **Better async stack traces** for workflow debugging

### Security

- **Latest security patches** and improvements
- **Enhanced cryptography** APIs
- **Improved vulnerability scanning** capabilities
- **Updated dependencies** with security fixes

### Long-term Support

.NET 10 is positioned for long-term support:

- **Extended support lifecycle** for enterprise deployments
- **Regular security updates** and patches
- **Community backing** and ecosystem growth
- **Forward compatibility** with future .NET versions

## Additional Resources

- [.NET 10 Release Notes](https://github.com/dotnet/core/tree/main/release-notes/10.0)
- [.NET 10 Breaking Changes](https://docs.microsoft.com/en-us/dotnet/core/compatibility/10.0)
- [Migration Guide](https://docs.microsoft.com/en-us/dotnet/core/migration/)
- [.NET Blog](https://devblogs.microsoft.com/dotnet/)

## Support

If you encounter issues during migration:

1. **Check the logs** in the `Logs/` directory
2. **Review error messages** carefully
3. **Search GitHub issues** for similar problems
4. **Open an issue** with detailed information:
   - .NET SDK version (`dotnet --version`)
   - Operating system
   - Error messages
   - Steps to reproduce

## Conclusion

The migration to .NET 10 is straightforward and provides significant benefits in performance, security, and developer experience. The FileScrubberMCP codebase is fully compatible with .NET 10, and all features continue to work as expected.

**Status**: ✅ **MIGRATION COMPLETE**

---

_Last Updated: November 15, 2025_
_Project Version: 1.0.0.1_
_Target Framework: net10.0_
