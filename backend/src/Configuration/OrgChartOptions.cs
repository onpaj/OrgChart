namespace OrgChart.API.Configuration;

/// <summary>
/// Configuration options for organizational chart data source
/// </summary>
public class OrgChartOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string SectionName = "OrgChart";

    /// <summary>
    /// Type of data source to use (Url, Database, FileSystem, etc.)
    /// </summary>
    public string DataSourceType { get; set; } = "Url";

    /// <summary>
    /// URL to the organizational structure JSON data source (used by UrlBasedDataSource)
    /// </summary>
    public string DataSourceUrl { get; set; } = string.Empty;

    /// <summary>
    /// Connection string for database data source (used by DatabaseDataSource)
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// File path for file system data source (used by FileSystemDataSource)
    /// </summary>
    public string? FilePath { get; set; }
}