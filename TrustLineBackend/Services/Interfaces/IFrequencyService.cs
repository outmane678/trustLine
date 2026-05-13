using AnonymousComplaintsAPI.DTOs.Responses;

namespace AnonymousComplaintsAPI.Services.Interfaces;

/// <summary>
/// Service for managing complaint frequencies
/// </summary>
public interface IFrequencyService
{
    /// <summary>
    /// Gets all frequencies, optionally including archived
    /// </summary>
    Task<IEnumerable<FrequencyResponse>> GetAllFrequenciesAsync(bool includeArchived = false);

    /// <summary>
    /// Gets a single frequency by ID
    /// </summary>
    Task<FrequencyResponse?> GetFrequencyAsync(int id);

    /// <summary>
    /// Creates a new frequency
    /// </summary>
    Task<FrequencyResponse> CreateFrequencyAsync(FrequencyResponse frequencyDto);

    /// <summary>
    /// Updates an existing frequency
    /// </summary>
    Task<FrequencyResponse> UpdateFrequencyAsync(int id, FrequencyResponse frequencyDto);

    /// <summary>
    /// Archives a frequency (soft delete)
    /// </summary>
    Task ArchiveFrequencyAsync(int id);

    /// <summary>
    /// Restores an archived frequency
    /// </summary>
    Task RestoreFrequencyAsync(int id);

    /// <summary>
    /// Permanently deletes a frequency
    /// </summary>
    Task DeleteFrequencyAsync(int id);
}
