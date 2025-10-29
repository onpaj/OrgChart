using OrgChart.API.Services;
using OrgChart.API.Configuration;
using OrgChart.API.DataSources;
using OrgChart.API.Repositories;

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
            ?? new[] { "http://localhost:3000" };
        
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Optional authentication
var authEnabled = builder.Configuration.GetValue<bool>("Authentication:Enabled");
if (authEnabled)
{
    builder.Services.AddAuthentication("Bearer")
           .AddJwtBearer("Bearer", options =>
           {
               // Configure JWT Bearer authentication
               options.Authority = builder.Configuration["Authentication:Authority"];
               options.TokenValidationParameters.ValidateAudience = false;
           });
    
    // Add authorization policies
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("OrgChartWritePolicy", policy =>
            policy.RequireClaim("OrgChart_Write"));
    });
}

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();

// Use authentication if enabled
if (authEnabled)
{
    app.UseAuthentication();
    app.UseAuthorization();
}

// Serve static files (React build output)
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

// Fallback to serve React app for SPA routing
app.MapFallbackToFile("index.html");

app.Run();

// Make the implicit Program class public for integration tests
public partial class Program { }