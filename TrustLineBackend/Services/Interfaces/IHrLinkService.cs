using AnonymousComplaintsAPI.DTOs.Responses;

namespace AnonymousComplaintsAPI.Services.Interfaces;

/// <summary>
/// Service for interacting with the HrLink external API
/// </summary>
public interface IHrLinkService
{
    /// <summary>
    /// Gets minimal profile information for all employees (used for dropdowns / user pickers)
    /// </summary>
    Task<List<ShortProfileResponseDto>?> GetProfilesMinimalAsync();

    /// <summary>
    /// Gets detailed profile information for a specific user by their User ID
    /// </summary>
    /// <param name="userId">The User ID to fetch profile for</param>
    /// <returns>Full profile data for the user, or null if not found</returns>
    Task<FullProfileResponseDto?> GetProfileByUserIdAsync(int userId);
}
