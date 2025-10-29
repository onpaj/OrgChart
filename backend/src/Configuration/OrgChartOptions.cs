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

    /// <summary>
    /// Repository permissions configuration
    /// </summary>
    public RepositoryPermissions Permissions { get; set; } = new();
}

/// <summary>
/// Configuration for repository operation permissions
/// </summary>
public class RepositoryPermissions
{
    /// <summary>
    /// Whether insert operations are enabled (creating new positions/employees)
    /// </summary>
    public bool InsertEnabled { get; set; } = false;

    /// <summary>
    /// Whether update operations are enabled (modifying existing positions/employees)
    /// </summary>
    public bool UpdateEnabled { get; set; } = false;

    /// <summary>
    /// Whether delete operations are enabled (removing positions/employees)
    /// </summary>
    public bool DeleteEnabled { get; set; } = false;
}