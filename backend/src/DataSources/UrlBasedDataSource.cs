using System.Text.Json;
using Microsoft.Extensions.Options;
using OrgChart.API.Configuration;
using OrgChart.API.Exceptions;
using OrgChart.API.Models;

namespace OrgChart.API.DataSources;

/// <summary>
/// URL-based data source implementation for organizational chart data
/// </summary>
public class UrlBasedDataSource : IOrgChartDataSource
{
    private readonly HttpClient _httpClient;
    private readonly OrgChartOptions _options;
    private readonly ILogger<UrlBasedDataSource> _logger;

    public UrlBasedDataSource(
        HttpClient httpClient,
        IOptions<OrgChartOptions> options,
        ILogger<UrlBasedDataSource> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<OrgChartResponse> GetDataAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching organizational structure from URL data source: {Url}", _options.DataSourceUrl);

            var response = await _httpClient.GetAsync(_options.DataSourceUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var orgChart = JsonSerializer.Deserialize<OrgChartResponse>(content, jsonOptions);

            if (orgChart == null)
            {
                _logger.LogError("Failed to deserialize organizational structure from URL: {Url}", _options.DataSourceUrl);
                throw new DataSourceException("Failed to deserialize organizational structure from URL data source");
            }

            _logger.LogDebug("Successfully fetched organizational structure from URL data source");
            return orgChart;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while fetching organizational structure from URL: {Url}", _options.DataSourceUrl);
            throw new DataSourceException($"Failed to fetch organizational structure from URL: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization error for organizational structure from URL: {Url}", _options.DataSourceUrl);
            throw new DataSourceException($"Failed to parse organizational structure from URL: {ex.Message}", ex);
        }
        catch (Exception ex) when (!(ex is DataSourceException))
        {
            _logger.LogError(ex, "Unexpected error while fetching organizational structure from URL: {Url}", _options.DataSourceUrl);
            throw new DataSourceException($"Unexpected error occurred while fetching data from URL: {ex.Message}", ex);
        }
    }
}