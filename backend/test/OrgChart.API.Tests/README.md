# OrgChart.API.Tests

This project contains comprehensive unit and integration tests for the OrgChart API.

## Test Structure

```
backend/test/OrgChart.API.Tests/
├── Unit/                          # Unit tests
│   ├── Controllers/               # Controller unit tests
│   │   └── OrgChartControllerTests.cs
│   └── Services/                  # Service unit tests
│       └── OrgChartServiceTests.cs
├── Integration/                   # Integration tests
│   └── OrgChartIntegrationTests.cs
├── TestHelpers/                   # Test utilities
│   ├── TestWebApplicationFactory.cs
│   └── TestDataBuilder.cs
├── appsettings.test.json          # Test configuration
└── GlobalUsings.cs                # Global using statements
```

## Test Categories

### Unit Tests

**OrgChartControllerTests**
- Tests authentication logic (enabled/disabled scenarios)
- Tests successful data retrieval
- Tests error handling and logging
- Tests authorization for authenticated users

**OrgChartServiceTests**
- Tests HTTP client interactions
- Tests JSON deserialization
- Tests error handling for network failures
- Tests logging behavior
- Tests cancellation token handling

### Integration Tests

**OrgChartIntegrationTests**
- Tests complete HTTP request/response flow
- Tests authentication integration
- Tests CORS configuration
- Tests error scenarios with external service failures
- Tests invalid JSON handling

## Running Tests

### Prerequisites
- .NET 8.0 SDK
- The main OrgChart.API project must be built first

### Commands

```bash
# Navigate to test directory
cd backend/test/OrgChart.API.Tests

# Restore dependencies
dotnet restore

# Build tests
dotnet build

# Run all tests
dotnet test

# Run tests with verbose output
dotnet test --verbosity normal

# Run only unit tests
dotnet test --filter "FullyQualifiedName~Unit"

# Run only integration tests
dotnet test --filter "FullyQualifiedName~Integration"

# Run with coverage (requires coverlet.msbuild package)
dotnet test --collect:"XPlat Code Coverage"
```

## Test Framework and Libraries

- **xUnit**: Test framework
- **Moq**: Mocking framework for unit tests
- **FluentAssertions**: Assertion library for more readable tests
- **Microsoft.AspNetCore.Mvc.Testing**: WebApplicationFactory for integration tests

## Test Configuration

The tests use `appsettings.test.json` for test-specific configuration:
- Authentication disabled by default
- Test API endpoints
- CORS settings for local development

## Mock Setup

### Unit Tests
- Mock `IOrgChartService` for controller tests
- Mock `HttpMessageHandler` for service tests
- Mock logging and configuration dependencies

### Integration Tests
- Use `WebApplicationFactory<Program>` for full application testing
- Override services and configuration as needed
- Mock external HTTP dependencies

## Key Test Scenarios

1. **Authentication Flow**
   - Tests with authentication enabled/disabled
   - Tests authorized and unauthorized access

2. **Data Retrieval**
   - Tests successful API calls
   - Tests data deserialization
   - Tests empty/minimal data scenarios

3. **Error Handling**
   - Network failures
   - Invalid JSON responses
   - Service exceptions
   - HTTP error status codes

4. **Logging**
   - Information logging for successful operations
   - Error logging for failures
   - Structured logging verification

## Common Issues

1. **Program class accessibility**: The main Program.cs includes `public partial class Program { }` to enable integration testing.

2. **HttpClient mocking**: Uses `Mock<HttpMessageHandler>` with protected setup for HTTP operations.

3. **Authentication testing**: Uses mock ClaimsIdentity and ClaimsPrincipal for auth scenarios.

## Adding New Tests

1. **Unit Tests**: Add to appropriate folder under `Unit/`
2. **Integration Tests**: Add to `Integration/` folder
3. **Test Data**: Use `TestDataBuilder` for consistent test data
4. **Mock Setup**: Follow existing patterns for mocking dependencies

## Sample Data Integration

The test project includes integration with the sample data file located at `/docs/sample-data.json`:

- **TestDataBuilder.CreateSampleDataResponse()**: Loads actual sample data from the file
- **TestDataBuilder.GetSampleDataJson()**: Returns the sample data as JSON string for HTTP mocking
- **Fallback mechanism**: If the sample data file is not found, tests fall back to programmatically created test data

This ensures tests are realistic and validate against actual data structures used by the application.