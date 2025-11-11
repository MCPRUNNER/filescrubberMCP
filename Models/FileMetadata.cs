using System.Text.Json.Serialization;

namespace filescrubberMCP.Models;

/// <summary>
/// Represents metadata information about a file
/// </summary>
public class FileMetadata
{
    /// <summary>
    /// Gets or sets the full path to the file
    /// </summary>
    [JsonPropertyName("full_path")]
    public string full_path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file name with extension
    /// </summary>
    [JsonPropertyName("file_name")]
    public string file_name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file name without extension
    /// </summary>
    [JsonPropertyName("file_name_without_extension")]
    public string file_name_without_extension { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file extension
    /// </summary>
    [JsonPropertyName("extension")]
    public string extension { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the directory path
    /// </summary>
    [JsonPropertyName("directory_path")]
    public string directory_path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file size in bytes
    /// </summary>
    [JsonPropertyName("size_in_bytes")]
    public long size_in_bytes
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the creation time (UTC)
    /// </summary>
    [JsonPropertyName("creation_time_utc")]
    public DateTime creation_time_utc
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the last write time (UTC)
    /// </summary>
    [JsonPropertyName("last_write_time_utc")]
    public DateTime last_write_time_utc
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the last access time (UTC)
    /// </summary>
    [JsonPropertyName("last_access_time_utc")]
    public DateTime last_access_time_utc
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the file attributes
    /// </summary>
    [JsonPropertyName("attributes")]
    public FileAttributes attributes
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets whether the file is read-only
    /// </summary>
    [JsonPropertyName("is_read_only")]
    public bool is_read_only
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets whether the file is hidden
    /// </summary>
    [JsonPropertyName("is_hidden")]
    public bool is_hidden
    {
        get; set;
    }
}
