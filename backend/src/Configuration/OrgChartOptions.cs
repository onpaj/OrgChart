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
    [Obsolete("Use UrlStorage.Url instead. This property is kept for backward compatibility.")]
    public string DataSourceUrl { get; set; } = string.Empty;

    /// <summary>
    /// URL-based storage configuration (used by UrlBasedDataSource)
    /// </summary>
    public UrlStorageOptions? UrlStorage { get; set; }

    /// <summary>
    /// Connection string for database data source (used by DatabaseDataSource)
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// File path for file system data source (used by FileSystemDataSource)
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// Azure Storage configuration (used by AzureStorageRepository)
    /// </summary>
    public AzureStorageOptions? AzureStorage { get; set; }

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

/// <summary>
/// Configuration options for Azure Storage
/// </summary>
public class AzureStorageOptions
{
    /// <summary>
    /// Azure Storage connection string
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Name of the blob container
    /// </summary>
    public string ContainerName { get; set; } = "orgchart";

    /// <summary>
    /// Name of the blob file containing the organizational chart data
    /// </summary>
    public string BlobName { get; set; } = "organization-structure.json";

    /// <summary>
    /// Whether to use managed identity instead of connection string
    /// </summary>
    public bool UseManagedIdentity { get; set; } = false;

    /// <summary>
    /// Storage account name (required when using managed identity)
    /// </summary>
    public string? StorageAccountName { get; set; }
}

/// <summary>
/// Configuration options for URL-based data source
/// </summary>
public class UrlStorageOptions
{
    /// <summary>
    /// URL to the organizational structure JSON data source
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Timeout for HTTP requests in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Number of retry attempts for failed requests
    /// </summary>
    public int RetryAttempts { get; set; } = 3;

    /// <summary>
    /// Static HTTP headers to include in all requests (use for non-sensitive headers like User-Agent)
    /// </summary>
    public Dictionary<string, string> StaticHeaders { get; set; } = new();

    /// <summary>
    /// Whether to validate SSL certificates
    /// </summary>
    public bool ValidateSslCertificate { get; set; } = true;
}