namespace OrgChart.API.Services;

/// <summary>
/// Background service that preloads and periodically refreshes user data from Microsoft Graph
/// </summary>
public class UserDataBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UserDataBackgroundService> _logger;
    private readonly IConfiguration _configuration;
    
    // Configuration settings
    private readonly TimeSpan _initialDelay;
    private readonly TimeSpan _refreshInterval;

    public UserDataBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<UserDataBackgroundService> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
        
        // Get configuration values or use defaults
        _initialDelay = TimeSpan.FromSeconds(
            _configuration.GetValue<int>("UserDataCache:InitialDelaySeconds", 30));
        _refreshInterval = TimeSpan.FromHours(
            _configuration.GetValue<int>("UserDataCache:RefreshIntervalHours", 6));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("User Data Background Service starting...");

        // Wait for initial delay to allow application to start up
        await Task.Delay(_initialDelay, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var userCacheService = scope.ServiceProvider.GetRequiredService<IUserCacheService>();
                
                _logger.LogInformation("Starting background user data refresh...");
                
                await userCacheService.PreloadAllUsersAsync();
                
                _logger.LogInformation("Background user data refresh completed. Next refresh in {Interval}",
                    _refreshInterval);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during background user data refresh");
            }

            // Wait for the next refresh interval
            try
            {
                await Task.Delay(_refreshInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when stopping the service
                break;
            }
        }

        _logger.LogInformation("User Data Background Service stopped.");
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("User Data Background Service is starting with initial delay: {InitialDelay}, refresh interval: {RefreshInterval}",
            _initialDelay, _refreshInterval);
        
        await base.StartAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("User Data Background Service is stopping...");
        
        await base.StopAsync(cancellationToken);
    }
}