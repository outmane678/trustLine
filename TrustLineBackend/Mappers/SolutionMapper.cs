using AnonymousComplaintsAPI.DTOs.Responses;
using AnonymousComplaintsAPI.Models.Entities;

namespace AnonymousComplaintsAPI.Mappers
{
    public static class SolutionMapper
    {
        public static SolutionResponse ToResponse(Solution entity, List<ShortProfileResponseDto> filtredProfiles)
        {
            if (entity == null)
                return null;

            var user = filtredProfiles?.FirstOrDefault(u => u.UserID == entity.CreatedBy);

            UserResponse createdByUser = null;
            if (user != null)
            {
                createdByUser = new UserResponse
                {
                    UserId = user.UserID,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Identifier = user.Identifier,
                    Avatar = user.Avatar
                };
            }

            return new SolutionResponse
            {
                SolutionID = entity.SolutionId,
                Content = entity.Content,
                AnonymousComplaintID = entity.AnonymousComplaintId,
                CreatedBy = entity.CreatedBy,
                CreatedAt = entity.CreatedAt,
                Archived = entity.Archived,
                CreatedByUser = createdByUser
            };
        }

        public static IEnumerable<SolutionResponse> ToResponseList(IEnumerable<Solution> entities, List<ShortProfileResponseDto> filtredProfiles)
        {
            return entities?.Select(e => ToResponse(e, filtredProfiles))
                   ?? Enumerable.Empty<SolutionResponse>();
        }

        public static Solution ToEntity(SolutionResponse dto, int? userId = null)
        {
            if (dto == null)
                return null;

            return new Solution
            {
                SolutionId = dto.SolutionID,
                Content = dto.Content,
                AnonymousComplaintId = dto.AnonymousComplaintID,
                CreatedBy = userId ?? dto.CreatedBy,
                CreatedAt = dto.CreatedAt ?? DateTime.UtcNow,
                Archived = dto.Archived
            };
        }

        public static void UpdateEntity(Solution entity, SolutionResponse dto)
        {
            if (entity == null || dto == null)
                return;

            entity.Content = dto.Content;
            entity.AnonymousComplaintId = dto.AnonymousComplaintID;
            entity.Archived = dto.Archived;
        }
    }
}
