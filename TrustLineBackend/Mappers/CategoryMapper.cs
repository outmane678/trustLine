using AnonymousComplaintsAPI.DTOs.Responses;
using AnonymousComplaintsAPI.DTOs.Requests;
using AnonymousComplaintsAPI.Models.Entities;

namespace AnonymousComplaintsAPI.Mappers
{
    public static class CategoryMapper
    {
        public static CategoryResponse ToResponse(Category entity, bool includeType = false)
        {
            if (entity == null)
                return null;

            return new CategoryResponse
            {
                Id = entity.CategoryId,
                Name = entity.Name,
                CreatedBy = entity.CreatedBy,
                CreatedAt = entity.CreatedAt,
                Archived = entity.Archived,
                TypeId = entity.TypeId,
                Type = includeType && entity.Type != null
                    ? TypeModelMapper.ToResponse(entity.Type, includeCategories: false)
                    : null,
                ParentCategoryId = entity.ParentCategoryId,
                ParentCategoryName = entity.ParentCategory?.Name
            };
        }

        public static IEnumerable<CategoryResponse> ToResponseList(IEnumerable<Category> entities, bool includeType = false)
        {
            return entities?.Select(e => ToResponse(e, includeType)) ?? Enumerable.Empty<CategoryResponse>();
        }

        public static Category ToEntity(CategoryResponse dto, int? userId = null)
        {
            if (dto == null)
                return null;

            return new Category
            {
                CategoryId = dto.Id,
                Name = dto.Name,
                CreatedBy = userId ?? dto.CreatedBy,
                CreatedAt = dto.CreatedAt ?? DateTime.UtcNow,
                Archived = dto.Archived,
                TypeId = dto.TypeId,
                ParentCategoryId = dto.ParentCategoryId
            };
        }

        public static Category CreateFromRequest(UpdateCategoryRequest request, int? userId = null)
        {
            if (request == null)
                return null;

            return new Category
            {
                Name = request.Name,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                Archived = false
            };
        }

        public static void UpdateEntity(Category entity, CategoryResponse dto, int? userId = null)
        {
            if (entity == null || dto == null)
                return;

            entity.Name = dto.Name;
            entity.CreatedBy = userId ?? dto.CreatedBy;
            entity.CreatedAt = dto.CreatedAt;
            entity.Archived = dto.Archived;
            entity.TypeId = dto.TypeId;
            entity.ParentCategoryId = dto.ParentCategoryId;
        }

        public static void UpdateFromRequest(Category entity, UpdateCategoryRequest request)
        {
            if (entity == null || request == null)
                return;

            entity.Name = request.Name;
            entity.ParentCategoryId = request.ParentCategoryId;
        }
    }
}
