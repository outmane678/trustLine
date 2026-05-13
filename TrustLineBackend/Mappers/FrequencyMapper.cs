using AnonymousComplaintsAPI.DTOs.Responses;
using AnonymousComplaintsAPI.Models.Entities;

namespace AnonymousComplaintsAPI.Mappers
{
    public static class FrequencyMapper
    {
        public static FrequencyResponse ToResponse(Frequency entity)
        {
            if (entity == null)
                return null;

            return new FrequencyResponse
            {
                FrequencyID = entity.FrequencyId,
                Label = entity.Label,
                CreatedBy = entity.CreatedBy,
                CreatedAt = entity.CreatedAt,
                Archived = entity.Archived
            };
        }

        public static IEnumerable<FrequencyResponse> ToResponseList(IEnumerable<Frequency> entities)
        {
            return entities?.Select(ToResponse) ?? Enumerable.Empty<FrequencyResponse>();
        }

        public static Frequency ToEntity(FrequencyResponse dto, int? userId = null)
        {
            if (dto == null)
                return null;

            return new Frequency
            {
                FrequencyId = dto.FrequencyID,
                Label = dto.Label,
                CreatedBy = userId ?? dto.CreatedBy,
                CreatedAt = dto.CreatedAt ?? DateTime.UtcNow,
                Archived = dto.Archived
            };
        }

        public static void UpdateEntity(Frequency entity, FrequencyResponse dto, int? userId = null)
        {
            if (entity == null || dto == null)
                return;

            entity.Label = dto.Label;
            entity.CreatedBy = userId ?? dto.CreatedBy;
            entity.CreatedAt = dto.CreatedAt;
            entity.Archived = dto.Archived;
        }
    }
}
