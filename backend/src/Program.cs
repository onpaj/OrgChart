using OrgChart.API.Services;
using OrgChart.API.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure OrgChart options
builder.Services.Configure<OrgChartOptions>(
    builder.Configuration.GetSection(OrgChartOptions.SectionName));

// Add HTTP client for external data fetching
builder.Services.AddHttpClient<IOrgChartService, OrgChartService>();

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