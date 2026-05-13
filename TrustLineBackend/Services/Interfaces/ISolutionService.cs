using AnonymousComplaintsAPI.DTOs.Requests;
using AnonymousComplaintsAPI.DTOs.Responses;

namespace AnonymousComplaintsAPI.Services.Interfaces;

/// <summary>
/// Service for managing solutions with orchestration logic for merged complaints
/// </summary>
public interface ISolutionService
{
    /// <summary>
    /// Gets a single solution by ID
    /// </summary>
    Task<SolutionResponse?> GetSolutionAsync(int id);

    /// <summary>
    /// Gets all non-archived solutions
    /// </summary>
    Task<IEnumerable<SolutionResponse>> GetAllSolutionsAsync();

    /// <summary>
    /// Gets all solutions (including archived)
    /// </summary>
    Task<IEnumerable<SolutionResponse>> GetAllSolutionsIncludingArchivedAsync();

    /// <summary>
    /// Gets all solutions for a specific complaint
    /// </summary>
    Task<IEnumerable<SolutionResponse>> GetSolutionsByComplaintAsync(int complaintId);

    /// <summary>
    /// Creates a solution for a complaint and updates complaint state to RESOLVED
    /// </summary>
    /// <param name="solutionDto">The solution data</param>
    Task<SolutionResponse> CreateSolutionAsync(SendResponseRequest request);

    /// <summary>
    /// Creates a solution for a complaint AND all its merged complaints,
    /// updating all complaint states to RESOLVED
    /// </summary>
    /// <param name="solutionDto">The solution data</param>
    Task<SolutionResponse> CreateSolutionForComplaintAndMergedAsync(SendResponseRequest request);

    /// <summary>
    /// Updates an existing solution
    /// </summary>
    Task<SolutionResponse> UpdateSolutionAsync(int id, SendResponseRequest request);

    /// <summary>
    /// Archives a solution (soft delete)
    /// </summary>
    Task ArchiveSolutionAsync(int id);

    /// <summary>
    /// Restores an archived solution
    /// </summary>
    Task RestoreSolutionAsync(int id);

    /// <summary>
    /// Permanently deletes a solution
    /// </summary>
    Task DeleteSolutionAsync(int id);
}
