using AnonymousComplaintsAPI.DTOs.Requests;
using AnonymousComplaintsAPI.DTOs.Responses;
using Microsoft.AspNetCore.Http;

namespace AnonymousComplaintsAPI.Services.Interfaces;

/// <summary>
/// Service for managing anonymous complaints with complex business logic
/// </summary>
public interface IAnonymousComplaintService
{
  
    Task<AnonymousComplaintResponse?> GetComplaintAsync(int id);

    //Task<IEnumerable<AnonymousComplaintResponse>> GetAllComplaintsAsync();

    //Task<IEnumerable<AnonymousComplaintResponse>> GetComplaintsByUserAsync(int userId);

    Task<AnonymousComplaintResponse> CreateComplaintWithAttachmentsAsync(
        CreateAnonymousComplaintRequest request,
        IFormFileCollection? files,
        int userId);
    Task<AnonymousComplaintResponse> UpdateComplaintWithAttachmentsAsync(
        int id,
        CreateAnonymousComplaintRequest request,
        IFormFileCollection? files);

    Task<int> MergeComplaintsAsync(List<int> complaintIds);

    /// <summary>
    /// Gets all complaints that are merged/fused with the specified complaint
    /// </summary>
    Task<IEnumerable<AnonymousComplaintResponse>> GetFusedComplaintsAsync(int complaintId);

    Task TransitionComplaintStateAsync(int id, string newState);
    Task ArchiveComplaintAsync(int id);
    Task RestoreComplaintAsync(int id);
    Task<SolutionResponse> AddComplaintResponseAsync(SendResponseRequest request);

    Task<DTOs.Responses.PaginatedResponse<AnonymousComplaintResponse>> GetComplaintsPaginatedAsync(DTOs.Requests.PaginationRequest request);
    Task<ComplaintsByUserResponse> GetComplaintsByUserAsync(int userId, DTOs.Requests.PaginationRequest request);

    Task<AnonymousComplaintResponse?> GetComplaintDetailsAsync(int id);
}
