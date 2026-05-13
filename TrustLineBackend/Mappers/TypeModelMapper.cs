using AnonymousComplaintsAPI.DTOs.Responses;
using AnonymousComplaintsAPI.Models.Entities;

namespace AnonymousComplaintsAPI.Mappers
{
    public static class TypeModelMapper
    {
        public static TypeModelResponse ToResponse(Models.Entities.Type entity, bool includeCategories = false)
        {
            if (entity == null)
                return null;

            return new TypeModelResponse
            {
                TypeId = entity.TypeId,
                Name = entity.Name,
                Archived = entity.Archived,
                CreatedAt = entity.CreatedAt,
                CreatedBy = entity.CreatedBy,
                Categories = includeCategories && entity.Categories != null
                    ? CategoryMapper.ToResponseList(entity.Categories, includeType: false).ToList()
                    : null
            };
        }

        public static IEnumerable<TypeModelResponse> ToResponseList(IEnumerable<Models.Entities.Type> entities, bool includeCategories = true)
        {
            return entities?.Select(e => ToResponse(e, includeCategories)) ?? Enumerable.Empty<TypeModelResponse>();
        }

        public static Models.Entities.Type ToEntity(TypeModelResponse dto, int? userId = null)
        {
            if (dto == null)
                return null;

            return new Models.Entities.Type
            {
                TypeId = dto.TypeId,
                Name = dto.Name,
                Archived = dto.Archived,
                CreatedAt = dto.CreatedAt ?? DateTime.UtcNow,
                CreatedBy = userId ?? dto.CreatedBy
            };
        }

        public static void UpdateEntity(Models.Entities.Type entity, TypeModelResponse dto, int? userId = null)
        {
            if (entity == null || dto == null)
                return;

            entity.Name = dto.Name;
            entity.Archived = dto.Archived;
            entity.CreatedBy = userId ?? dto.CreatedBy;
        }
    }
}
