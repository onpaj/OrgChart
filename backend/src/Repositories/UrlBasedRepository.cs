using Microsoft.Extensions.Options;
using OrgChart.API.Configuration;
using OrgChart.API.DataSources;
using OrgChart.API.Models;

namespace OrgChart.API.Repositories;

/// <summary>
/// URL-based repository implementation for organizational chart data
/// This is a read-only repository that uses IOrgChartDataSource for data fetching
/// </summary>
public class UrlBasedRepository : IOrgChartRepository
{
    private readonly IOrgChartDataSource _dataSource;
    private readonly RepositoryPermissions _permissions;
    private readonly ILogger<UrlBasedRepository> _logger;

    public UrlBasedRepository(
        IOrgChartDataSource dataSource,
        IOptions<OrgChartOptions> options,
        ILogger<UrlBasedRepository> logger)
    {
        _dataSource = dataSource;
        _permissions = options.Value.Permissions;
        _logger = logger;
    }

    /// <inheritdoc />
    public bool InsertEnabled => _permissions.InsertEnabled;

    /// <inheritdoc />
    public bool UpdateEnabled => _permissions.UpdateEnabled;

    /// <inheritdoc />
    public bool DeleteEnabled => _permissions.DeleteEnabled;

    /// <inheritdoc />
    public async Task<OrgChartResponse> GetDataAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting organizational chart data from URL-based repository");
        return await _dataSource.GetDataAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<Position> CreatePositionAsync(Position position, CancellationToken cancellationToken = default)
    {
        if (!InsertEnabled)
        {
            _logger.LogWarning("Attempted to create position when InsertEnabled is false");
            throw new NotSupportedException("Insert operations are not enabled for this repository. Enable 'OrgChart:Permissions:InsertEnabled' in configuration.");
        }

        _logger.LogWarning("URL-based repository does not support position creation - operation not implemented");
        throw new NotImplementedException("URL-based repository is read-only. Position creation is not supported.");
    }

    /// <inheritdoc />
    public Task<Position> UpdatePositionAsync(Position position, CancellationToken cancellationToken = default)
    {
        if (!UpdateEnabled)
        {
            _logger.LogWarning("Attempted to update position when UpdateEnabled is false");
            throw new NotSupportedException("Update operations are not enabled for this repository. Enable 'OrgChart:Permissions:UpdateEnabled' in configuration.");
        }

        _logger.LogWarning("URL-based repository does not support position updates - operation not implemented");
        throw new NotImplementedException("URL-based repository is read-only. Position updates are not supported.");
    }

    /// <inheritdoc />
    public Task DeletePositionAsync(string positionId, CancellationToken cancellationToken = default)
    {
        if (!DeleteEnabled)
        {
            _logger.LogWarning("Attempted to delete position when DeleteEnabled is false");
            throw new NotSupportedException("Delete operations are not enabled for this repository. Enable 'OrgChart:Permissions:DeleteEnabled' in configuration.");
        }

        _logger.LogWarning("URL-based repository does not support position deletion - operation not implemented");
        throw new NotImplementedException("URL-based repository is read-only. Position deletion is not supported.");
    }

    /// <inheritdoc />
    public Task<Employee> CreateEmployeeAsync(string positionId, Employee employee, CancellationToken cancellationToken = default)
    {
        if (!InsertEnabled)
        {
            _logger.LogWarning("Attempted to create employee when InsertEnabled is false");
            throw new NotSupportedException("Insert operations are not enabled for this repository. Enable 'OrgChart:Permissions:InsertEnabled' in configuration.");
        }

        _logger.LogWarning("URL-based repository does not support employee creation - operation not implemented");
        throw new NotImplementedException("URL-based repository is read-only. Employee creation is not supported.");
    }

    /// <inheritdoc />
    public Task<Employee> UpdateEmployeeAsync(string positionId, Employee employee, CancellationToken cancellationToken = default)
    {
        if (!UpdateEnabled)
        {
            _logger.LogWarning("Attempted to update employee when UpdateEnabled is false");
            throw new NotSupportedException("Update operations are not enabled for this repository. Enable 'OrgChart:Permissions:UpdateEnabled' in configuration.");
        }

        _logger.LogWarning("URL-based repository does not support employee updates - operation not implemented");
        throw new NotImplementedException("URL-based repository is read-only. Employee updates are not supported.");
    }

    /// <inheritdoc />
    public Task DeleteEmployeeAsync(string positionId, string employeeId, CancellationToken cancellationToken = default)
    {
        if (!DeleteEnabled)
        {
            _logger.LogWarning("Attempted to delete employee when DeleteEnabled is false");
            throw new NotSupportedException("Delete operations are not enabled for this repository. Enable 'OrgChart:Permissions:DeleteEnabled' in configuration.");
        }

        _logger.LogWarning("URL-based repository does not support employee deletion - operation not implemented");
        throw new NotImplementedException("URL-based repository is read-only. Employee deletion is not supported.");
    }
}