using OrgChart.API.Models;

namespace OrgChart.API.Repositories;

/// <summary>
/// Repository interface for organizational chart data with CRUD operations
/// </summary>
public interface IOrgChartRepository
{
    /// <summary>
    /// Indicates whether insert operations are enabled for this repository
    /// </summary>
    bool InsertEnabled { get; }

    /// <summary>
    /// Indicates whether update operations are enabled for this repository
    /// </summary>
    bool UpdateEnabled { get; }

    /// <summary>
    /// Indicates whether delete operations are enabled for this repository
    /// </summary>
    bool DeleteEnabled { get; }

    /// <summary>
    /// Retrieves the complete organizational chart data
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Organization chart data</returns>
    Task<OrgChartResponse> GetDataAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new position in the organizational chart
    /// </summary>
    /// <param name="position">Position to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created position with assigned ID</returns>
    /// <exception cref="NotSupportedException">Thrown when InsertEnabled is false</exception>
    Task<Position> CreatePositionAsync(Position position, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing position in the organizational chart
    /// </summary>
    /// <param name="position">Position to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated position</returns>
    /// <exception cref="NotSupportedException">Thrown when UpdateEnabled is false</exception>
    Task<Position> UpdatePositionAsync(Position position, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a position from the organizational chart
    /// </summary>
    /// <param name="positionId">ID of the position to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <exception cref="NotSupportedException">Thrown when DeleteEnabled is false</exception>
    Task DeletePositionAsync(string positionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new employee in a specific position
    /// </summary>
    /// <param name="positionId">ID of the position</param>
    /// <param name="employee">Employee to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created employee with assigned ID</returns>
    /// <exception cref="NotSupportedException">Thrown when InsertEnabled is false</exception>
    Task<Employee> CreateEmployeeAsync(string positionId, Employee employee, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing employee
    /// </summary>
    /// <param name="positionId">ID of the position</param>
    /// <param name="employee">Employee to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated employee</returns>
    /// <exception cref="NotSupportedException">Thrown when UpdateEnabled is false</exception>
    Task<Employee> UpdateEmployeeAsync(string positionId, Employee employee, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an employee from a position
    /// </summary>
    /// <param name="positionId">ID of the position</param>
    /// <param name="employeeId">ID of the employee to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <exception cref="NotSupportedException">Thrown when DeleteEnabled is false</exception>
    Task DeleteEmployeeAsync(string positionId, string employeeId, CancellationToken cancellationToken = default);
}