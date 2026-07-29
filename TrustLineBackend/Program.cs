// ============================================================================
// AnonymousComplaintsAPI - TRUSTLINE
// ============================================================================
// A complaint management system with anonymous reporting capabilities.
// This application follows the standardized backend architecture with:
// - Repository Pattern for data access
// - Service Layer for business logic
// - DTOs for data transfer
// - Middleware for cross-cutting concerns
// - Centralized configuration
// ============================================================================

// Global using statements
global using AnonymousComplaintsAPI.Models;

// Type aliases for moved classes (backward compatibility)
global using JwtSettings = AnonymousComplaintsAPI.Configurations.JwtSettings;
global using AppConfig = AnonymousComplaintsAPI.Configurations.AppConfig;
global using UserTokens = AnonymousComplaintsAPI.DTOs.Responses.UserTokenResponse;
global using UserLogins = AnonymousComplaintsAPI.DTOs.Requests.UserLoginRequest;
global using ErrorViewModel = AnonymousComplaintsAPI.DTOs.Responses.ErrorViewModel;

using Microsoft.Extensions.WebEncoders;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.Extensions.FileProviders;
using OfficeOpenXml;
using AnonymousComplaintsAPI.Models.Entities;
using AnonymousComplaintsAPI.Services;
using AnonymousComplaintsAPI.Extensions;
using AnonymousComplaintsAPI.Configurations;
using AnonymousComplaintsAPI.Middlewares;
using AnonymousComplaintsAPI.Services.EnsureServices;


var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// SERVICE REGISTRATION
// ============================================================================

// Core ASP.NET Services
builder.Services.AddControllers();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();

// Excel Package License Configuration
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

// Web Encoder Configuration (support for all Unicode ranges)
builder.Services.Configure<WebEncoderOptions>(options =>
{
    options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All);
});

// File Upload Configuration
var uploadsDirectory = builder.Configuration.GetSection("UploadsDirectory").Get<string>();
builder.Services.AddSingleton(uploadsDirectory);

// Legacy Services (backward compatibility - to be migrated)


// ============================================================================
// DATABASE CONFIGURATION
// ============================================================================
builder.Services.AddDatabaseConfiguration(builder.Configuration);

// ============================================================================
// AUTHENTICATION & AUTHORIZATION
// ============================================================================
builder.Services.AddJwtConfiguration(builder.Configuration);

// ============================================================================
// API DOCUMENTATION & CORS
// ============================================================================
builder.Services.AddSwaggerConfiguration();
builder.Services.AddCorsConfiguration();

// ============================================================================
// APPLICATION SERVICES & REPOSITORIES
// ============================================================================
// Registers all repositories (data access layer)
builder.Services.AddRepositories();

// Registers all application services (business logic layer)
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddScoped<IEnsureService, EnsureService>();


var app = builder.Build();

var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
if (!Directory.Exists(uploadsPath))
    Directory.CreateDirectory(uploadsPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});
// ============================================================================
// MIDDLEWARE PIPELINE
// ============================================================================
// Order matters! Middleware is executed in the order registered.

// 1. Swagger/OpenAPI (must be early in pipeline to serve UI)
app.UseSwaggerConfiguration();

// 2. Error Handling (must be early to catch all exceptions)
app.UseErrorHandling();

// 3. Request Logging (logs all requests with performance metrics)
app.UseRequestLogging();

// 4. Static Files
app.UseStaticFiles();

// 5. HTTPS Redirection (only in production)
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// 6. Routing (required for endpoint mapping)
app.UseRouting();

// 7. CORS (Cross-Origin Resource Sharing)
app.UseCors();

// 8. Authentication (must be before Authorization)
app.UseAuthentication();

// 9. Authorization
app.UseAuthorization();

// 10. Endpoint Mapping
// Map API controllers (no default route since Swagger UI is at root)
app.MapControllers();

// Start the application
app.Run();

// Make Program accessible for integration tests (WebApplicationFactory<Program>)
public partial class Program { }
