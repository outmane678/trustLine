using AnonymousComplaintsAPI.DTOs.AccessGate;
using AnonymousComplaintsAPI.Services.Interfaces;
using System.Text.Json;

namespace AnonymousComplaintsAPI.Services.Implementations;

/// <summary>
/// Service that calls AccessGate's external API to check user permissions.
/// Uses the /api/external/permissions/check endpoint.
/// </summary>
public class AccessGateService : IAccessGateService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AccessGateService> _logger;
    private readonly string _baseUrl;
    private readonly string _token;
    private readonly string _appCode;
    private readonly string _checkPermissionEndpoint;

    public AccessGateService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<AccessGateService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
        _baseUrl = _configuration.GetValue<string>("ExternalApis:AccessGate:BaseUrl") ?? "http://10.200.0.222:10100";
        _token = _configuration.GetValue<string>("ExternalApis:AccessGate:Token") ?? "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ0b2tlbl90eXBlIjoic2VydmljZSIsImp0aSI6ImVjODViMTA4LWIzMWMtNDYxNC1hMTk3LWQxNWY1NTIwNzVhNiIsImFwcF9pZCI6IjMiLCJhcHBfbmFtZSI6IkFjY2Vzc0dhdGUiLCJ1c2VyX2lkIjoiMTEwOSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJzdmNfQWNjZXNzR2F0ZSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6InN2Y19BY2Nlc3NHYXRlQHNlcnZpY2UuYWNjZXNzZ2F0ZS5sb2NhbCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTEwOSIsImlhdCI6IjE3NzI2MzQ2NjciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL2V4cGlyYXRpb24iOiJUaHVyc2RheSwgTWFyY2ggNCwgMjAyNyIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6WyJhZy1leHQtcmVhZC11c2VycyIsImFnLWV4dC1jcmVhdGUtdXNlcnMiLCJhZy1leHQtcmVhZC1yb2xlcyIsImFnLWV4dC1yZWFkLXBlcm1pc3Npb25zIiwiYWctZXh0LWNoZWNrLXBlcm1pc3Npb25zIiwiYWctZXh0LXJlYWQtZGVsZWdhdGlvbiIsImFnLWV4dC1yZWFkLXByb2plY3RzIiwiYWctZXh0LW1hbmFnZS1wcm9qZWN0cyIsImFnLWV4dC1tYW5hZ2UtdXNlcnMiXSwicm9sZV9uYW1lIjoiRXh0ZXJuYWwgU2VydmljZSIsInBlcm1pc3Npb25zIjoiW1wiYWctZXh0LXJlYWQtdXNlcnNcIixcImFnLWV4dC1jcmVhdGUtdXNlcnNcIixcImFnLWV4dC1yZWFkLXJvbGVzXCIsXCJhZy1leHQtcmVhZC1wZXJtaXNzaW9uc1wiLFwiYWctZXh0LWNoZWNrLXBlcm1pc3Npb25zXCIsXCJhZy1leHQtcmVhZC1kZWxlZ2F0aW9uXCIsXCJhZy1leHQtcmVhZC1wcm9qZWN0c1wiLFwiYWctZXh0LW1hbmFnZS1wcm9qZWN0c1wiLFwiYWctZXh0LW1hbmFnZS11c2Vyc1wiXSIsInJvbGVzIjoiW1wiRXh0ZXJuYWwgU2VydmljZVwiXSIsIm5iZiI6MTc3MjYzNDY2NywiZXhwIjoxODA0MTcwNjY3LCJpc3MiOiJodHRwczovL2xvY2FsaG9zdDo3MjUxIiwiYXVkIjoiaHR0cHM6Ly9sb2NhbGhvc3Q6NzI1MSJ9.NccddDP5_kXlE5klcPMtxPh_xRHVWfnJl4sFkOdBJ6A";
        _appCode = _configuration.GetValue<string>("ExternalApis:AccessGate:AppCode") ?? "TrustLine";
        _checkPermissionEndpoint = _configuration.GetValue<string>("ExternalApis:AccessGate:Endpoints:CheckPermission") ?? "/api/external/permissions/check";
    }

    /// <inheritdoc/>
    public async Task<bool> CheckPermissionAsync(int userId, string permission)
    {
        try
        {
            var url = $"{_baseUrl}{_checkPermissionEndpoint}?userId={userId}&permission={Uri.EscapeDataString(permission)}&appCode={Uri.EscapeDataString(_appCode)}";

            _logger.LogDebug("AccessGate permission check: userId={UserId}, permission={Permission}, url={Url}", userId, permission, url);

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);

            if (!string.IsNullOrWhiteSpace(_token))
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
            }

            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<PermissionCheckResponse>(content, options);

                if (result != null)
                {
                    _logger.LogDebug("AccessGate permission check result: userId={UserId}, permission={Permission}, hasPermission={HasPermission}",
                        userId, permission, result.HasPermission);
                    return result.HasPermission;
                }
            }
            else
            {
                _logger.LogWarning("AccessGate permission check failed with status {StatusCode} for userId={UserId}, permission={Permission}",
                    response.StatusCode, userId, permission);
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission via AccessGate: userId={UserId}, permission={Permission}", userId, permission);
            return false;
        }
    }
}
