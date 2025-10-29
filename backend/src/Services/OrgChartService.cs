using OrgChart.API.Exceptions;
using OrgChart.API.Models;
using OrgChart.API.Repositories;

namespace OrgChart.API.Services;

/// <summary>
/// Service for retrieving organizational chart data from configured repository
/// </summary>
public class OrgChartService : IOrgChartService
{
    private readonly IOrgChartRepository _repository;
    private readonly ILogger<OrgChartService> _logger;

    public OrgChartService(
        IOrgChartRepository repository,
        ILogger<OrgChartService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<OrgChartResponse> GetOrganizationStructureAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving organizational structure from repository");

            var orgChart = await _repository.GetDataAsync(cancellationToken);

            _logger.LogInformation(
                "Successfully loaded organizational structure: {PositionCount} positions, {EmployeeCount} employees (Repository permissions: Insert={InsertEnabled}, Update={UpdateEnabled}, Delete={DeleteEnabled})",
                orgChart.Organization.Positions.Count,
                orgChart.Organization.Positions.Sum(p => p.Employees.Count),
                _repository.InsertEnabled,
                _repository.UpdateEnabled,
                _repository.DeleteEnabled);

            return orgChart;
        }
        catch (DataSourceException ex)
        {
            _logger.LogError(ex, "Repository error while retrieving organizational structure");
            throw new InvalidOperationException($"Failed to retrieve organizational structure: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while retrieving organizational structure");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<Position> CreatePositionAsync(CreatePositionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating new position: {Title}", request.Title);
            
            if (!_repository.InsertEnabled)
            {
                throw new NotSupportedException("Insert operations are not enabled for this repository");
            }
            
            var position = new Position
            {
                Id = Guid.NewGuid().ToString(),
                Title = request.Title,
                Description = request.Description,
                ParentPositionId = request.ParentPositionId,
                Department = request.Department,
                Url = request.Url,
                Employees = new List<Employee>()
            };

            var createdPosition = await _repository.CreatePositionAsync(position, cancellationToken);
            _logger.LogInformation("Position created with ID: {PositionId}", createdPosition.Id);
            
            return createdPosition;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating position: {Title}", request.Title);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<Position> UpdatePositionAsync(string id, UpdatePositionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating position: {PositionId}", id);
            
            if (!_repository.UpdateEnabled)
            {
                throw new NotSupportedException("Update operations are not enabled for this repository");
            }
            
            var position = new Position
            {
                Id = id,
                Title = request.Title,
                Description = request.Description,
                ParentPositionId = request.ParentPositionId,
                Department = request.Department,
                Url = request.Url,
                Employees = new List<Employee>()
            };

            var updatedPosition = await _repository.UpdatePositionAsync(position, cancellationToken);
            _logger.LogInformation("Position updated: {PositionId}", id);
            
            return updatedPosition;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating position: {PositionId}", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task DeletePositionAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting position: {PositionId}", id);
            
            if (!_repository.DeleteEnabled)
            {
                throw new NotSupportedException("Delete operations are not enabled for this repository");
            }
            
            await _repository.DeletePositionAsync(id, cancellationToken);
            _logger.LogInformation("Position deleted: {PositionId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting position: {PositionId}", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<Employee> CreateEmployeeAsync(CreateEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating new employee: {Name} in position {PositionId}", request.Name, request.PositionId);
            
            if (!_repository.InsertEnabled)
            {
                throw new NotSupportedException("Insert operations are not enabled for this repository");
            }
            
            var employee = new Employee
            {
                Id = Guid.NewGuid().ToString(),
                Name = request.Name,
                Email = request.Email,
                StartDate = request.StartDate,
                Url = request.Url
            };

            var createdEmployee = await _repository.CreateEmployeeAsync(request.PositionId, employee, cancellationToken);
            _logger.LogInformation("Employee created with ID: {EmployeeId}", createdEmployee.Id);
            
            return createdEmployee;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating employee: {Name}", request.Name);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<Employee> UpdateEmployeeAsync(string id, UpdateEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating employee: {EmployeeId}", id);
            
            if (!_repository.UpdateEnabled)
            {
                throw new NotSupportedException("Update operations are not enabled for this repository");
            }
            
            var employee = new Employee
            {
                Id = id,
                Name = request.Name,
                Email = request.Email,
                StartDate = request.StartDate,
                Url = request.Url
            };

            var updatedEmployee = await _repository.UpdateEmployeeAsync(request.PositionId, employee, cancellationToken);
            _logger.LogInformation("Employee updated: {EmployeeId}", id);
            
            return updatedEmployee;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating employee: {EmployeeId}", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task DeleteEmployeeAsync(string positionId, string employeeId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting employee: {EmployeeId} from position: {PositionId}", employeeId, positionId);
            
            if (!_repository.DeleteEnabled)
            {
                throw new NotSupportedException("Delete operations are not enabled for this repository");
            }
            
            await _repository.DeleteEmployeeAsync(positionId, employeeId, cancellationToken);
            _logger.LogInformation("Employee deleted: {EmployeeId}", employeeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting employee: {EmployeeId}", employeeId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task DeleteEmployeeAsync(string employeeId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Finding position for employee: {EmployeeId}", employeeId);
            var positionId = await FindPositionByEmployeeIdAsync(employeeId, cancellationToken);
            
            await DeleteEmployeeAsync(positionId, employeeId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting employee: {EmployeeId}", employeeId);
            throw;
        }
    }

    /// <summary>
    /// Finds the position ID for a given employee ID
    /// </summary>
    /// <param name="employeeId">Employee ID to search for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Position ID containing the employee</returns>
    /// <exception cref="InvalidOperationException">Thrown when employee is not found</exception>
    private async Task<string> FindPositionByEmployeeIdAsync(string employeeId, CancellationToken cancellationToken = default)
    {
        var orgChart = await _repository.GetDataAsync(cancellationToken);
        
        foreach (var position in orgChart.Organization.Positions)
        {
            if (position.Employees.Any(e => e.Id == employeeId))
            {
                return position.Id;
            }
        }
        
        throw new InvalidOperationException($"Employee with ID {employeeId} not found in any position");
    }
}