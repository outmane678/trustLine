using AnonymousComplaintsAPI.DTOs.Responses;
using Microsoft.AspNetCore.Http;

namespace AnonymousComplaintsAPI.Services.Interfaces;

/// <summary>
/// Service for managing file attachments with file I/O operations
/// </summary>
public interface IAttachmentService
{
    /// <summary>
    /// Gets a single attachment by ID
    /// </summary>
    Task<AttachmentResponse?> GetAttachmentAsync(int id);

    /// <summary>
    /// Gets all non-archived attachments
    /// </summary>
    Task<IEnumerable<AttachmentResponse>> GetAllAttachmentsAsync();

    /// <summary>
    /// Gets all attachments for a specific complaint
    /// </summary>
    Task<IEnumerable<AttachmentResponse>> GetAttachmentsByComplaintAsync(int complaintId);

    /// <summary>
    /// Uploads a file and creates an attachment record
    /// </summary>
    /// <param name="file">The file to upload</param>
    /// <param name="complaintId">Optional complaint ID to associate with</param>
    Task<AttachmentResponse> UploadAttachmentAsync(IFormFile file, int? complaintId = null);

    /// <summary>
    /// Generates a ZIP archive of all attachments for a complaint
    /// </summary>
    /// <param name="complaintId">The complaint ID</param>
    /// <returns>Stream containing the ZIP file</returns>
    Task<Stream> GenerateComplaintAttachmentsZipAsync(int complaintId);

    /// <summary>
    /// Updates an attachment record
    /// </summary>
    Task<AttachmentResponse> UpdateAttachmentAsync(int id, AttachmentResponse attachmentDto);

    /// <summary>
    /// Creates an attachment record
    /// </summary>
    Task<AttachmentResponse> CreateAttachmentAsync(AttachmentResponse attachmentDto);

    /// <summary>
    /// Archives a single attachment
    /// </summary>
    Task ArchiveAttachmentAsync(int id);

    /// <summary>
    /// Archives multiple attachments belonging to a complaint
    /// </summary>
    Task ArchiveMultipleAttachmentsAsync(int complaintId, List<int> attachmentIds);

    /// <summary>
    /// Restores an archived attachment
    /// </summary>
    Task RestoreAttachmentAsync(int id);

    /// <summary>
    /// Permanently deletes an attachment and its file
    /// </summary>
    Task DeleteAttachmentAsync(int id);
}
