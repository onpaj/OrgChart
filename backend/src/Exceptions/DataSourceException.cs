namespace OrgChart.API.Exceptions;

/// <summary>
/// Exception thrown when data source operations fail
/// </summary>
public class DataSourceException : Exception
{
    /// <summary>
    /// Initializes a new instance of the DataSourceException class
    /// </summary>
    public DataSourceException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the DataSourceException class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public DataSourceException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the DataSourceException class with a specified error message and inner exception
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public DataSourceException(string message, Exception innerException) : base(message, innerException)
    {
    }
}