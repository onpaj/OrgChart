using Azure;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using OrgChart.API.Configuration;
using OrgChart.API.Models;
using System.Text.Json;

namespace OrgChart.API.Repositories;

/// <summary>
/// Repository implementation that stores organizational chart data in Azure Blob Storage as JSON
/// </summary>
public class AzureStorageOrgChartRepository : IOrgChartRepository
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly AzureStorageOptions _storageOptions;
    private readonly RepositoryPermissions _permissions;
    private readonly SemaphoreSlim _semaphore;
    private readonly ILogger<AzureStorageOrgChartRepository> _logger;

    public AzureStorageOrgChartRepository(
        IOptions<OrgChartOptions> options,
        ILogger<AzureStorageOrgChartRepository> logger)
    {
        _logger = logger;
        
        var orgChartOptions = options.Value;
        _storageOptions = orgChartOptions.AzureStorage ?? throw new ArgumentException("Azure Storage configuration is required");
        _permissions = orgChartOptions.Permissions;
        
        // Initialize blob service client
        if (_storageOptions.UseManagedIdentity)
        {
            if (string.IsNullOrEmpty(_storageOptions.StorageAccountName))
                throw new ArgumentException("StorageAccountName is required when using managed identity");
                
            var storageUri = new Uri($"https://{_storageOptions.StorageAccountName}.blob.core.windows.net");
            _blobServiceClient = new BlobServiceClient(storageUri, new DefaultAzureCredential());
        }
        else
        {
            if (string.IsNullOrEmpty(_storageOptions.ConnectionString))
                throw new ArgumentException("ConnectionString is required when not using managed identity");
                
            _blobServiceClient = new BlobServiceClient(_storageOptions.ConnectionString);
        }

        _semaphore = new SemaphoreSlim(1, 1);
    }

    public bool InsertEnabled => _permissions.InsertEnabled;
    public bool UpdateEnabled => _permissions.UpdateEnabled;
    public bool DeleteEnabled => _permissions.DeleteEnabled;

    public async Task<OrgChartResponse> GetDataAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_storageOptions.ContainerName);
            var blobClient = containerClient.GetBlobClient(_storageOptions.BlobName);

            if (!await blobClient.ExistsAsync(cancellationToken))
            {
                _logger.LogInformation("Blob {BlobName} does not exist, returning empty organization", _storageOptions.BlobName);
                return new OrgChartResponse();
            }

            var response = await blobClient.DownloadContentAsync(cancellationToken);
            var jsonContent = response.Value.Content.ToString();

            var orgChart = JsonSerializer.Deserialize<OrgChartResponse>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return orgChart ?? new OrgChartResponse();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading organizational chart data from Azure Storage");
            throw;
        }
    }

    public async Task<Position> CreatePositionAsync(Position position, CancellationToken cancellationToken = default)
    {
        if (!InsertEnabled)
            throw new NotSupportedException("Insert operations are not enabled for this repository");

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var orgChart = await GetDataAsync(cancellationToken);
            
            // Generate new ID if not provided
            if (string.IsNullOrEmpty(position.Id))
            {
                position.Id = Guid.NewGuid().ToString();
            }

            // Validate that position ID is unique
            if (orgChart.Organization.Positions.Any(p => p.Id == position.Id))
            {
                throw new InvalidOperationException($"Position with ID '{position.Id}' already exists");
            }

            // Validate parent position exists if specified
            if (!string.IsNullOrEmpty(position.ParentPositionId))
            {
                if (!orgChart.Organization.Positions.Any(p => p.Id == position.ParentPositionId))
                {
                    throw new InvalidOperationException($"Parent position with ID '{position.ParentPositionId}' does not exist");
                }
            }

            orgChart.Organization.Positions.Add(position);
            await SaveDataAsync(orgChart, cancellationToken);

            _logger.LogInformation("Created position {PositionId} with title {Title}", position.Id, position.Title);
            return position;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<Position> UpdatePositionAsync(Position position, CancellationToken cancellationToken = default)
    {
        if (!UpdateEnabled)
            throw new NotSupportedException("Update operations are not enabled for this repository");

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var orgChart = await GetDataAsync(cancellationToken);
            
            var existingPosition = orgChart.Organization.Positions.FirstOrDefault(p => p.Id == position.Id);
            if (existingPosition == null)
            {
                throw new InvalidOperationException($"Position with ID '{position.Id}' not found");
            }

            // Validate parent position exists if specified and different from current position
            if (!string.IsNullOrEmpty(position.ParentPositionId) && position.ParentPositionId != position.Id)
            {
                if (!orgChart.Organization.Positions.Any(p => p.Id == position.ParentPositionId))
                {
                    throw new InvalidOperationException($"Parent position with ID '{position.ParentPositionId}' does not exist");
                }

                // Check for circular reference
                if (WouldCreateCircularReference(orgChart.Organization.Positions, position.Id, position.ParentPositionId))
                {
                    throw new InvalidOperationException("Update would create a circular reference in the hierarchy");
                }
            }

            // Update the position properties
            existingPosition.Title = position.Title;
            existingPosition.Description = position.Description;
            existingPosition.Level = position.Level;
            existingPosition.ParentPositionId = position.ParentPositionId;
            existingPosition.Department = position.Department;
            existingPosition.Url = position.Url;

            await SaveDataAsync(orgChart, cancellationToken);

            _logger.LogInformation("Updated position {PositionId} with title {Title}", position.Id, position.Title);
            return existingPosition;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task DeletePositionAsync(string positionId, CancellationToken cancellationToken = default)
    {
        if (!DeleteEnabled)
            throw new NotSupportedException("Delete operations are not enabled for this repository");

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var orgChart = await GetDataAsync(cancellationToken);
            
            var position = orgChart.Organization.Positions.FirstOrDefault(p => p.Id == positionId);
            if (position == null)
            {
                throw new InvalidOperationException($"Position with ID '{positionId}' not found");
            }

            // Check if any positions have this as their parent
            var childPositions = orgChart.Organization.Positions.Where(p => p.ParentPositionId == positionId).ToList();
            if (childPositions.Any())
            {
                throw new InvalidOperationException($"Cannot delete position '{positionId}' because it has child positions: {string.Join(", ", childPositions.Select(p => p.Id))}");
            }

            orgChart.Organization.Positions.Remove(position);
            await SaveDataAsync(orgChart, cancellationToken);

            _logger.LogInformation("Deleted position {PositionId}", positionId);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<Employee> CreateEmployeeAsync(string positionId, Employee employee, CancellationToken cancellationToken = default)
    {
        if (!InsertEnabled)
            throw new NotSupportedException("Insert operations are not enabled for this repository");

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var orgChart = await GetDataAsync(cancellationToken);
            
            var position = orgChart.Organization.Positions.FirstOrDefault(p => p.Id == positionId);
            if (position == null)
            {
                throw new InvalidOperationException($"Position with ID '{positionId}' not found");
            }

            // Generate new ID if not provided
            if (string.IsNullOrEmpty(employee.Id))
            {
                employee.Id = Guid.NewGuid().ToString();
            }

            // Validate that employee ID is unique across all positions
            if (orgChart.Organization.Positions.Any(p => p.Employees.Any(e => e.Id == employee.Id)))
            {
                throw new InvalidOperationException($"Employee with ID '{employee.Id}' already exists");
            }

            position.Employees.Add(employee);
            await SaveDataAsync(orgChart, cancellationToken);

            _logger.LogInformation("Created employee {EmployeeId} with name {Name} in position {PositionId}", employee.Id, employee.Name, positionId);
            return employee;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<Employee> UpdateEmployeeAsync(string positionId, Employee employee, CancellationToken cancellationToken = default)
    {
        if (!UpdateEnabled)
            throw new NotSupportedException("Update operations are not enabled for this repository");

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var orgChart = await GetDataAsync(cancellationToken);
            
            var position = orgChart.Organization.Positions.FirstOrDefault(p => p.Id == positionId);
            if (position == null)
            {
                throw new InvalidOperationException($"Position with ID '{positionId}' not found");
            }

            var existingEmployee = position.Employees.FirstOrDefault(e => e.Id == employee.Id);
            if (existingEmployee == null)
            {
                throw new InvalidOperationException($"Employee with ID '{employee.Id}' not found in position '{positionId}'");
            }

            // Update the employee properties
            existingEmployee.Name = employee.Name;
            existingEmployee.Email = employee.Email;
            existingEmployee.StartDate = employee.StartDate;
            existingEmployee.IsPrimary = employee.IsPrimary;
            existingEmployee.Url = employee.Url;

            await SaveDataAsync(orgChart, cancellationToken);

            _logger.LogInformation("Updated employee {EmployeeId} with name {Name} in position {PositionId}", employee.Id, employee.Name, positionId);
            return existingEmployee;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task DeleteEmployeeAsync(string positionId, string employeeId, CancellationToken cancellationToken = default)
    {
        if (!DeleteEnabled)
            throw new NotSupportedException("Delete operations are not enabled for this repository");

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var orgChart = await GetDataAsync(cancellationToken);
            
            var position = orgChart.Organization.Positions.FirstOrDefault(p => p.Id == positionId);
            if (position == null)
            {
                throw new InvalidOperationException($"Position with ID '{positionId}' not found");
            }

            var employee = position.Employees.FirstOrDefault(e => e.Id == employeeId);
            if (employee == null)
            {
                throw new InvalidOperationException($"Employee with ID '{employeeId}' not found in position '{positionId}'");
            }

            position.Employees.Remove(employee);
            await SaveDataAsync(orgChart, cancellationToken);

            _logger.LogInformation("Deleted employee {EmployeeId} from position {PositionId}", employeeId, positionId);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task SaveDataAsync(OrgChartResponse orgChart, CancellationToken cancellationToken)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_storageOptions.ContainerName);
            
            // Ensure container exists
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);
            
            var blobClient = containerClient.GetBlobClient(_storageOptions.BlobName);

            var jsonContent = JsonSerializer.Serialize(orgChart, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonContent));
            
            var blobOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = "application/json"
                }
            };

            await blobClient.UploadAsync(stream, blobOptions, cancellationToken);

            _logger.LogDebug("Successfully saved organizational chart data to Azure Storage");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving organizational chart data to Azure Storage");
            throw;
        }
    }

    private static bool WouldCreateCircularReference(List<Position> positions, string positionId, string newParentId)
    {
        var visited = new HashSet<string>();
        var current = newParentId;

        while (!string.IsNullOrEmpty(current) && visited.Add(current))
        {
            if (current == positionId)
                return true;

            var position = positions.FirstOrDefault(p => p.Id == current);
            current = position?.ParentPositionId;
        }

        return false;
    }

    public void Dispose()
    {
        _semaphore?.Dispose();
    }
}