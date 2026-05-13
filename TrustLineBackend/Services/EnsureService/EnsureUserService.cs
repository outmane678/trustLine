
using AnonymousComplaintsAPI.Models;
using AnonymousComplaintsAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;
using AnonymousComplaintsAPI.DTOs.Responses;
using AnonymousComplaintsAPI.Data;
using AnonymousComplaintsAPI.Services.Interfaces;

namespace AnonymousComplaintsAPI.Services.EnsureServices
{
    public interface IEnsureService
    {
        Task<User> EnsureUserExistsAsync(int userId);
        Task<List<ShortProfileResponseDto>?> GetExternalProfilesAsync();
    }

    public class EnsureService : IEnsureService
    {
        private readonly AnonymousComplaintsV002Context _context;
        private readonly IHrLinkService _hrLinkService;
        private readonly ILogger<EnsureService> _logger;

        // Simple in-memory cache
        private static List<ShortProfileResponseDto>? _cachedProfiles;
        private static DateTime? _cacheExpiry;
        private static readonly SemaphoreSlim _cacheLock = new SemaphoreSlim(1, 1);
        private const int CACHE_DURATION_MINUTES = 10;

        public EnsureService(
            AnonymousComplaintsV002Context context,
            IHrLinkService hrLinkService,
            ILogger<EnsureService> logger)
        {
            _context = context;
            _hrLinkService = hrLinkService;
            _logger = logger;
        }

        public async Task<User> EnsureUserExistsAsync(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);

            if (user != null)
                return user;

            user = new User { UserId = id };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<List<ShortProfileResponseDto>?> GetExternalProfilesAsync()
        {
            try
            {
                if (_cachedProfiles != null && _cacheExpiry.HasValue && DateTime.Now < _cacheExpiry.Value)
                    return _cachedProfiles;

                await _cacheLock.WaitAsync();
                try
                {
                    // Double-check after acquiring lock
                    if (_cachedProfiles != null && _cacheExpiry.HasValue && DateTime.Now < _cacheExpiry.Value)
                        return _cachedProfiles;

                    var profiles = await _hrLinkService.GetProfilesMinimalAsync();

                    if (profiles != null)
                    {
                        _cachedProfiles = profiles;
                        _cacheExpiry = DateTime.Now.AddMinutes(CACHE_DURATION_MINUTES);
                    }

                    return profiles;
                }
                finally
                {
                    _cacheLock.Release();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching external profiles from HrLink");
                return null;
            }


        }
    }
}
