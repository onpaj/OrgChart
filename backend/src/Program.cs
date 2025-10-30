using OrgChart.API.Services;
using OrgChart.API.Configuration;
using OrgChart.API.DataSources;
using OrgChart.API.Repositories;
using OrgChart.API.Extensions;
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

// Register the service
builder.Services.AddScoped<IOrgChartService, OrgChartService>();

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
        
    // Role-based policies
    options.AddPolicy("OrgChartReader", policy => 
        policy.RequireClaim("scp", "access_as_user"));
        
    options.AddPolicy("OrgChartWriter", policy => 
        policy.RequireClaim("scp", "OrgChart_Admin"));
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