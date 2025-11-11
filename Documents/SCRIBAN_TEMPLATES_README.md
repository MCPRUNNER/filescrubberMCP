# Scriban Template Examples for File Listing

This directory contains example Scriban templates that work with the output from the `fscrub_file_list` MCP tool.

## Available Templates

### 1. `file_list_report.sbn` - Comprehensive Report

A detailed markdown report that includes:

- File listing table with size, extension, and last modified date
- Summary statistics (total size, average size, largest/smallest files)
- Files grouped by extension
- Detailed information for each file

### 2. `simple_file_list.sbn` - Simple Summary

A basic template that lists files with their sizes.

## Sample Data

`file_list_sample_data.json` - Example JSON output from the `fscrub_file_list` tool that can be used to test the templates.

## Usage Example

### Using MCP Tools

1. **List files in a directory:**

```
fscrub_file_list(directoryPath="C:\\Projects\\MyApp", searchPattern="*.cs", recursive=true)
```

2. **Process the template with the output:**

```
scriban_process_template(
    templateFilePath="Examples/file_list_report.sbn",
    jsonData=<output_from_step_1>,
    outputFilePath="Reports/file_report.md"
)
```

### Using the Service Directly

```csharp
// Get file listing
var fileListJson = await fileTools.ListFiles("C:\\Projects\\MyApp", "*.cs", true);

// Process template
var result = await templateService.ProcessTemplateAsync(
    "Examples/file_list_report.sbn",
    fileListJson,
    "Reports/file_report.md"
);
```

## JSON Data Structure

The `fscrub_file_list` tool returns JSON in the following format:

```json
{
  "success": true,
  "directoryPath": "C:\\Projects\\MyApp",
  "searchPattern": "*.cs",
  "recursive": true,
  "fileCount": 3,
  "files": [
    {
      "full_path": "C:\\Projects\\MyApp\\Program.cs",
      "file_name": "Program.cs",
      "file_name_without_extension": "Program",
      "extension": ".cs",
      "directory_path": "C:\\Projects\\MyApp",
      "size_in_bytes": 2048,
      "creation_time_utc": "2024-01-15T10:30:00Z",
      "last_write_time_utc": "2024-01-20T14:45:00Z",
      "last_access_time_utc": "2024-01-21T09:15:00Z",
      "attributes": "Archive",
      "is_read_only": false,
      "is_hidden": false
    }
  ]
}
```

## Available Template Variables

When processing these templates, you have access to:

- `success` - Boolean indicating if the file listing was successful
- `directoryPath` - The directory that was searched
- `searchPattern` - The search pattern used (e.g., "\*.cs")
- `recursive` - Boolean indicating if search was recursive
- `fileCount` - Number of files found
- `files` - Array of file objects, each containing:
  - `full_path` - Complete file path
  - `file_name` - File name with extension
  - `file_name_without_extension` - File name only
  - `extension` - File extension (e.g., ".cs")
  - `directory_path` - Parent directory path
  - `size_in_bytes` - File size in bytes
  - `creation_time_utc` - UTC creation timestamp
  - `last_write_time_utc` - UTC last modified timestamp
  - `last_access_time_utc` - UTC last accessed timestamp
  - `attributes` - File attributes
  - `is_read_only` - Boolean for read-only status
  - `is_hidden` - Boolean for hidden status

## Scriban Functions Used

These templates demonstrate various Scriban features:

- **Loops:** `{{ for file in files }}`
- **Conditionals:** `{{ if fileCount > 0 }}`
- **Math functions:** `{{ math.sum }}`, `{{ math.format }}`
- **Array functions:** `{{ array.map }}`, `{{ array.sort }}`, `{{ array.group_by }}`
- **Date formatting:** `{{ date.to_string }}`
- **Object access:** `{{ object.file_name }}`

## Creating Your Own Templates

You can create custom templates using any of the available data. Common use cases:

1. **Generate documentation** from code files
2. **Create inventory reports** of project files
3. **Build file manifests** for deployment
4. **Analyze project structure** and file organization
5. **Generate change logs** based on file modification dates

## Testing Templates

Use the provided `file_list_sample_data.json` to test your templates without running the file listing tool:

```
scriban_process_template(
    templateFilePath="Examples/your_template.sbn",
    jsonData=<contents_of_file_list_sample_data.json>,
    outputFilePath="test_output.md"
)
```
