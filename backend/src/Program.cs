using OrgChart.API.Services;
using OrgChart.API.Configuration;
using OrgChart.API.DataSources;
using OrgChart.API.Repositories;
using OrgChart.API.Extensions;
using OrgChart.API.Authorization;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure OrgChart options
builder.Services.Configure<OrgChartOptions>(
    builder.Configuration.GetSection(OrgChartOptions.SectionName));

// Register data source and repository based on configuration
var dataSourceType = builder.Configuration.GetValue<string>("OrgChart:DataSourceType") ?? "Url";
switch (dataSourceType.ToLower())
{
    case "url":
        // Register data source
        builder.Services.AddHttpClient<IOrgChartDataSource, UrlBasedDataSource>();
        // Register repository
        builder.Services.AddScoped<IOrgChartRepository, UrlBasedRepository>();
        break;
    case "azurestorage":
        // Register Azure Storage repository (no data source needed as repository handles storage directly)
        builder.Services.AddScoped<IOrgChartRepository, AzureStorageOrgChartRepository>();
        break;
    // Add more cases for future data sources and repositories:
    // case "database":
    //     builder.Services.AddScoped<DatabaseDataSource>();
    //     builder.Services.AddScoped<IOrgChartRepository, DatabaseRepository>();
    //     break;
    // case "filesystem":
    //     builder.Services.AddScoped<FileSystemDataSource>();
    //     builder.Services.AddScoped<IOrgChartRepository, FileSystemRepository>();
    //     break;
    default:
        // Default to URL-based
        builder.Services.AddHttpClient<IOrgChartDataSource, UrlBasedDataSource>();
        builder.Services.AddScoped<IOrgChartRepository, UrlBasedRepository>();
        break;
}

// Register the services
builder.Services.AddScoped<IOrgChartService, OrgChartService>();

// Register permission service based on authentication mode
var useMockAuth = builder.Configuration.GetValue<bool>("UseMockAuth", false);
if (useMockAuth)
{
    builder.Services.AddScoped<IUserPermissionService, MockUserPermissionService>();
    builder.Services.AddScoped<IAuthorizationHandler, MockOrgChartAuthorizationHandler>();
}
else
{
    builder.Services.AddScoped<IUserPermissionService, RoleBaseUserPermissionService>();
    builder.Services.AddScoped<IAuthorizationHandler, OrgChartAuthorizationHandler>();
}

// Register authorization handler

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
            ?? new[] { "http://localhost:3001" }; // Updated to match frontend port
        
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Configure authentication
var loggerFactory = LoggerFactory.Create(logBuilder => logBuilder.AddConsole());
var logger = loggerFactory.CreateLogger("Authentication");
builder.Services.ConfigureAuthentication(builder, logger);

// Add authorization
builder.Services.AddAuthorization(options =>
{
    // Don't set FallbackPolicy - let individual controllers/actions decide
    // This allows [AllowAnonymous] to work properly
        
    // Policy for reading org chart data
    options.AddPolicy(OrgChartPolicies.Read, policy => 
        policy.Requirements.Add(new OrgChartRequirement(OrgChartAccessLevel.Read)));
        
    // Policy for editing org chart data - requires admin role
    options.AddPolicy(OrgChartPolicies.Write, policy => 
        policy.Requirements.Add(new OrgChartRequirement(OrgChartAccessLevel.Write)));
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();

// Configure authentication middleware
app.UseAuthentication();
app.UseAuthorization();

// Serve static files (React build output)
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

// Fallback to serve React app for SPA routing
app.MapFallbackToFile("index.html");

app.Run();

// Make the implicit Program class public for integration tests
public partial class Program { }