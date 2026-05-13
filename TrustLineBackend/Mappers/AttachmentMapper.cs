using AnonymousComplaintsAPI.DTOs.Responses;
using AnonymousComplaintsAPI.Models.Entities;

namespace AnonymousComplaintsAPI.Mappers
{
    public static class AttachmentMapper
    {
        public static AttachmentResponse ToResponse(Attachment entity)
        {
            if (entity == null)
                return null;

            return new AttachmentResponse
            {
                Id = entity.AttachmentId,
                FileName = entity.FileName,
                FilePath = entity.FilePath,
                FileType = entity.FileType,
                AnonymousComplaintID = entity.AnonymousComplaintId,
                CreatedBy = entity.CreatedBy,
                CreatedAt = entity.CreatedAt,
                Archived = entity.Archived
            };
        }

        public static IEnumerable<AttachmentResponse> ToResponseList(IEnumerable<Attachment> entities)
        {
            return entities?.Select(ToResponse) ?? Enumerable.Empty<AttachmentResponse>();
        }

        public static Attachment ToEntity(AttachmentResponse dto, int? userId = null)
        {
            if (dto == null)
                return null;

            return new Attachment
            {
                AttachmentId = dto.Id,
                FileName = dto.FileName,
                FilePath = dto.FilePath,
                FileType = dto.FileType,
                AnonymousComplaintId = dto.AnonymousComplaintID,
                CreatedBy = userId ?? dto.CreatedBy,
                CreatedAt = dto.CreatedAt ?? DateTime.UtcNow,
                Archived = dto.Archived
            };
        }

        public static void UpdateEntity(Attachment entity, AttachmentResponse dto)
        {
            if (entity == null || dto == null)
                return;

            entity.FileName = dto.FileName;
            entity.FilePath = dto.FilePath;
            entity.FileType = dto.FileType;
            entity.AnonymousComplaintId = dto.AnonymousComplaintID;
            entity.CreatedAt = dto.CreatedAt;
            entity.Archived = dto.Archived;
        }
    }
}
