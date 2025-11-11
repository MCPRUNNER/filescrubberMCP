namespace filescrubberMCP.Models;

/// <summary>
/// Sample data model
/// </summary>
public class SampleData
{
    public int Id
    {
        get; set;
    }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedDate
    {
        get; set;
    }
}
