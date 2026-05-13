using AnonymousComplaintsAPI.DTOs.Responses;
using AnonymousComplaintsAPI.Services.Interfaces;
using System.Text.Json;

namespace AnonymousComplaintsAPI.Services.Implementations;

public class HrLinkService : IHrLinkService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<HrLinkService> _logger;
    private readonly string _baseUrl;
    private readonly string _token;
    private readonly string _getProfilesMinimalEndpoint;
    private readonly string _getProfileByUserIdEndpoint;

    public HrLinkService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<HrLinkService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
        _baseUrl = _configuration.GetValue<string>("ExternalApis:HrLink:BaseUrl") ?? "http://10.200.0.222:8000";
        _token = _configuration.GetValue<string>("ExternalApis:HrLink:Token") ?? "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ0b2tlbl90eXBlIjoic2VydmljZSIsImp0aSI6IjE3NTY4N2RmLThlNWUtNDFhMS04OTllLTYwM2Y4MDg1ZDAyMyIsImFwcF9pZCI6IjEzIiwiYXBwX25hbWUiOiJIckxpbmsiLCJ1c2VyX2lkIjoiMTEyMCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJzdmNfSHJMaW5rIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIjoic3ZjX0hyTGlua0BzZXJ2aWNlLmFjY2Vzc2dhdGUubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjExMjAiLCJpYXQiOiIxNzcyODA4MDgyIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9leHBpcmF0aW9uIjoiU2F0dXJkYXksIE1hcmNoIDYsIDIwMjciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOlsiaHItZXh0LXJlYWQtcHJvZmlsZXMiLCJoci1leHQtcmVhZC1mdW5jdGlvbnMiLCJoci1leHQtcmVhZC1kZXBhcnRtZW50cyJdLCJyb2xlX25hbWUiOiJFeHRlcm5hbCBTZXJ2aWNlIiwicGVybWlzc2lvbnMiOiJbXCJoci1leHQtcmVhZC1wcm9maWxlc1wiLFwiaHItZXh0LXJlYWQtZnVuY3Rpb25zXCIsXCJoci1leHQtcmVhZC1kZXBhcnRtZW50c1wiXSIsInJvbGVzIjoiW1wiRXh0ZXJuYWwgU2VydmljZVwiXSIsIm5iZiI6MTc3MjgwODA4MiwiZXhwIjoxODA0MzQ0MDgyLCJpc3MiOiJodHRwczovL2xvY2FsaG9zdDo3MjUxIiwiYXVkIjoiaHR0cHM6Ly9sb2NhbGhvc3Q6NzI1MSJ9.LQ4F8cbcJBtZN89Q77SI_3BFXWiCEf_afLT8Ce-UI4Q";
        _getProfilesMinimalEndpoint = _configuration.GetValue<string>("ExternalApis:HrLink:Endpoints:GetProfilesMinimal") ?? "/api/external/Profiles/minimal";
        _getProfileByUserIdEndpoint = _configuration.GetValue<string>("ExternalApis:HrLink:Endpoints:GetProfileByUserId") ?? "/api/external/profiles/ByUserId/{UserId}";
    }

    /// <inheritdoc/>
    public async Task<List<ShortProfileResponseDto>?> GetProfilesMinimalAsync()
    {
        try
        {
            var url = $"{_baseUrl}{_getProfilesMinimalEndpoint}";
            _logger.LogInformation("Fetching minimal profiles from HrLink: {Url}", url);

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);

            if (!string.IsNullOrWhiteSpace(_token))
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
            }

            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var profiles = JsonSerializer.Deserialize<List<ShortProfileResponseDto>>(content, options);
                return profiles;
            }
            else
            {
                _logger.LogWarning("HrLink API returned status {StatusCode} for GetProfilesMinimal",
                    response.StatusCode);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching minimal profiles from HrLink");
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<FullProfileResponseDto?> GetProfileByUserIdAsync(int userId)
    {
        try
        {
            var endpoint = _getProfileByUserIdEndpoint.Replace("{UserId}", userId.ToString());
            var url = $"{_baseUrl}{endpoint}";
            _logger.LogInformation("Fetching profile from HrLink for User ID {UserId}: {Url}", userId, url);

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);

            if (!string.IsNullOrWhiteSpace(_token))
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
            }

            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var profile = JsonSerializer.Deserialize<FullProfileResponseDto>(content, options);
                return profile;
            }
            else
            {
                _logger.LogWarning("HrLink API returned status {StatusCode} for GetProfileByUserId with User ID {UserId}",
                    response.StatusCode, userId);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching profile from HrLink for User ID {UserId}", userId);
            return null;
        }
    }
}
