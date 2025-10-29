using System.Text.Json;
using Microsoft.Extensions.Options;
using OrgChart.API.Configuration;
using OrgChart.API.Exceptions;
using OrgChart.API.Models;
using Polly;

namespace OrgChart.API.DataSources;

/// <summary>
/// URL-based data source implementation for organizational chart data
/// </summary>
public class UrlBasedDataSource : IOrgChartDataSource
{
    private readonly HttpClient _httpClient;
    private readonly UrlStorageOptions _urlOptions;
    private readonly ILogger<UrlBasedDataSource> _logger;
    private readonly ResiliencePipeline _resiliencePipeline;

    public UrlBasedDataSource(
        HttpClient httpClient,
        IOptions<OrgChartOptions> options,
        ILogger<UrlBasedDataSource> logger)
    {
        _httpClient = httpClient;
        var orgChartOptions = options.Value;
        
        // Support both new UrlStorage configuration and legacy DataSourceUrl for backward compatibility
        _urlOptions = orgChartOptions.UrlStorage ?? new UrlStorageOptions 
        { 
            Url = orgChartOptions.DataSourceUrl 
        };
        
        if (string.IsNullOrEmpty(_urlOptions.Url))
        {
            throw new ArgumentException("URL configuration is required. Set either UrlStorage.Url or DataSourceUrl (legacy).");
        }
        
        _logger = logger;
        
        // Configure HttpClient with timeout and static headers only
        _httpClient.Timeout = TimeSpan.FromSeconds(_urlOptions.TimeoutSeconds);
        
        foreach (var header in _urlOptions.StaticHeaders)
        {
            _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
        }

        // Configure Polly resilience pipeline
        _resiliencePipeline = new ResiliencePipelineBuilder()
            .AddRetry(new Polly.Retry.RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>(),
                MaxRetryAttempts = _urlOptions.RetryAttempts,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                OnRetry = args =>
                {
                    _logger.LogWarning("Retrying HTTP request (attempt {Attempt}) due to: {Exception}", 
                        args.AttemptNumber + 1, args.Outcome.Exception?.Message);
                    return ValueTask.CompletedTask;
                }
            })
            .AddTimeout(TimeSpan.FromSeconds(_urlOptions.TimeoutSeconds))
            .Build();
    }

    /// <inheritdoc />
    public async Task<OrgChartResponse> GetDataAsync(CancellationToken cancellationToken = default)
    {
        return await GetDataAsync(new Dictionary<string, string>(), cancellationToken);
    }

    /// <inheritdoc />
    public async Task<OrgChartResponse> GetDataAsync(Dictionary<string, string> forwardedHeaders, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching organizational structure from URL data source: {Url}", _urlOptions.Url);

            var result = await _resiliencePipeline.ExecuteAsync(async (ct) =>
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, _urlOptions.Url);
                
                // Add forwarded headers (like Authorization) to this specific request
                foreach (var header in forwardedHeaders)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

                var response = await _httpClient.SendAsync(request, ct);
                response.EnsureSuccessStatusCode();

                return response;
            }, cancellationToken);

            var content = await result.Content.ReadAsStringAsync(cancellationToken);

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var orgChart = JsonSerializer.Deserialize<OrgChartResponse>(content, jsonOptions);

            if (orgChart == null)
            {
                _logger.LogError("Failed to deserialize organizational structure from URL: {Url}", _urlOptions.Url);
                throw new DataSourceException("Failed to deserialize organizational structure from URL data source");
            }

            _logger.LogDebug("Successfully fetched organizational structure from URL data source");
            return orgChart;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while fetching organizational structure from URL: {Url}", _urlOptions.Url);
            throw new DataSourceException($"Failed to fetch organizational structure from URL: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization error for organizational structure from URL: {Url}", _urlOptions.Url);
            throw new DataSourceException($"Failed to parse organizational structure from URL: {ex.Message}", ex);
        }
        catch (Exception ex) when (!(ex is DataSourceException))
        {
            _logger.LogError(ex, "Unexpected error while fetching organizational structure from URL: {Url}", _urlOptions.Url);
            throw new DataSourceException($"Unexpected error occurred while fetching data from URL: {ex.Message}", ex);
        }
    }
}