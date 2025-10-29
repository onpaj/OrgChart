using OrgChart.API.Models;

namespace OrgChart.API.Services;

/// <summary>
/// Service for retrieving and managing organizational chart data
/// </summary>
public interface IOrgChartService
{
    /// <summary>
    /// Retrieves the complete organizational structure from external data source
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Organization chart data</returns>
    Task<OrgChartResponse> GetOrganizationStructureAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Creates a new position in the organizational structure
    /// </summary>
    /// <param name="request">Position creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created position</returns>
    Task<Position> CreatePositionAsync(CreatePositionRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates an existing position in the organizational structure
    /// </summary>
    /// <param name="id">Position ID</param>
    /// <param name="request">Position update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated position</returns>
    Task<Position> UpdatePositionAsync(string id, UpdatePositionRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes a position from the organizational structure
    /// </summary>
    /// <param name="id">Position ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeletePositionAsync(string id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Creates a new employee
    /// </summary>
    /// <param name="request">Employee creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created employee</returns>
    Task<Employee> CreateEmployeeAsync(CreateEmployeeRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates an existing employee
    /// </summary>
    /// <param name="id">Employee ID</param>
    /// <param name="request">Employee update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated employee</returns>
    Task<Employee> UpdateEmployeeAsync(string id, UpdateEmployeeRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes an employee
    /// </summary>
    /// <param name="positionId">Position ID</param>
    /// <param name="employeeId">Employee ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteEmployeeAsync(string positionId, string employeeId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes an employee by finding their position automatically
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteEmployeeAsync(string employeeId, CancellationToken cancellationToken = default);
}