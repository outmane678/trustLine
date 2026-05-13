using AnonymousComplaintsAPI.DTOs.Responses;
using AnonymousComplaintsAPI.Mappers;
using AnonymousComplaintsAPI.Models.Entities;
using AnonymousComplaintsAPI.Repositories.Interfaces;
using AnonymousComplaintsAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace AnonymousComplaintsAPI.Services.Implementations;

public class AttachmentService : IAttachmentService
{
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IAnonymousComplaintRepository _complaintRepository;
    private readonly IFileService _fileService;
    private readonly ILogger<AttachmentService> _logger;

    public AttachmentService(IAttachmentRepository attachmentRepository, IAnonymousComplaintRepository complaintRepository,
        IFileService fileService, ILogger<AttachmentService> logger)
    {
        _attachmentRepository = attachmentRepository;
        _complaintRepository = complaintRepository;
        _fileService = fileService;
        _logger = logger;
    }

    public async Task<AttachmentResponse?> GetAttachmentAsync(int id)
    {
        var attachment = await _attachmentRepository.GetByIdAsync(id);
        return attachment != null ? AttachmentMapper.ToResponse(attachment) : null;
    }

    public async Task<IEnumerable<AttachmentResponse>> GetAllAttachmentsAsync()
    {
        var attachments = await _attachmentRepository.GetNonArchivedAsync();
        return AttachmentMapper.ToResponseList(attachments);
    }

    public async Task<IEnumerable<AttachmentResponse>> GetAttachmentsByComplaintAsync(int complaintId)
    {
        var attachments = await _attachmentRepository.GetByComplaintIdAsync(complaintId);
        return AttachmentMapper.ToResponseList(attachments);
    }

    public async Task<AttachmentResponse> UploadAttachmentAsync(IFormFile file, int? complaintId = null)
    {
        var filePath = await _fileService.SaveFileAsync(file, "Uploads");
        var attachment = new Attachment
        {
            FileName = file.FileName,
            FilePath = filePath,
            AnonymousComplaintId = complaintId,
            Archived = false,
            CreatedAt = DateTime.Now,
            CreatedBy = 1
        };
        var created = await _attachmentRepository.CreateAsync(attachment);
        return AttachmentMapper.ToResponse(created);
    }

    public async Task<Stream> GenerateComplaintAttachmentsZipAsync(int complaintId)
    {
        var attachments = await _attachmentRepository.GetByComplaintIdAsync(complaintId);
        var filePaths = attachments.Where(a => !string.IsNullOrEmpty(a.FilePath)).Select(a => a.FilePath!).ToList();
        var existingFiles = filePaths.Where(path => _fileService.FileExists(path)).ToList();
        return await _fileService.CreateZipFromFilesAsync(existingFiles, $"Complaint_{complaintId}_Attachments.zip");
    }

    public async Task<AttachmentResponse> UpdateAttachmentAsync(int id, AttachmentResponse attachmentDto)
    {
        var attachment = await _attachmentRepository.GetByIdAsync(id);
        if (attachment == null) throw new KeyNotFoundException($"Attachment {id} not found");
        attachment.FileName = attachmentDto.FileName;
        attachment.AnonymousComplaintId = attachmentDto.AnonymousComplaintID;
        await _attachmentRepository.UpdateAsync(attachment);
        return AttachmentMapper.ToResponse(attachment);
    }

    public async Task<AttachmentResponse> CreateAttachmentAsync(AttachmentResponse attachmentDto)
    {
        var attachment = new Attachment
        {
            FileName = attachmentDto.FileName,
            FilePath = attachmentDto.FilePath,
            AnonymousComplaintId = attachmentDto.AnonymousComplaintID,
            Archived = false,
            CreatedAt = DateTime.Now,
            CreatedBy = attachmentDto.CreatedBy ?? 1
        };
        var created = await _attachmentRepository.CreateAsync(attachment);
        return AttachmentMapper.ToResponse(created);
    }

    public async Task ArchiveAttachmentAsync(int id) => await _attachmentRepository.ArchiveAsync(id);

    public async Task ArchiveMultipleAttachmentsAsync(int complaintId, List<int> attachmentIds)
    {
        foreach (var attachmentId in attachmentIds)
        {
            var attachment = await _attachmentRepository.GetByIdAsync(attachmentId);
            if (attachment?.AnonymousComplaintId != complaintId)
                throw new InvalidOperationException($"Attachment {attachmentId} does not belong to complaint {complaintId}");
        }
        await _attachmentRepository.ArchiveBatchAsync(attachmentIds);
    }

    public async Task RestoreAttachmentAsync(int id) => await _attachmentRepository.RestoreAsync(id);

    public async Task DeleteAttachmentAsync(int id)
    {
        var attachment = await _attachmentRepository.GetByIdAsync(id);
        if (attachment != null && !string.IsNullOrEmpty(attachment.FilePath))
            await _fileService.DeleteFileAsync(attachment.FilePath);
        await _attachmentRepository.DeleteAsync(id);
    }
}
