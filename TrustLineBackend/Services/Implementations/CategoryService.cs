using AnonymousComplaintsAPI.DTOs.Requests;
using AnonymousComplaintsAPI.DTOs.Responses;
using AnonymousComplaintsAPI.Mappers;
using AnonymousComplaintsAPI.Models.Entities;
using AnonymousComplaintsAPI.Repositories.Interfaces;
using AnonymousComplaintsAPI.Services.Interfaces;

namespace AnonymousComplaintsAPI.Services.Implementations;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(ICategoryRepository categoryRepository, ILogger<CategoryService> logger)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<CategoryResponse>> GetAllCategoriesAsync(bool includeArchived = false)
    {
        var categories = includeArchived
            ? await _categoryRepository.GetAllWithTypeAsync()
            : await _categoryRepository.GetNonArchivedAsync();
        return CategoryMapper.ToResponseList(categories);
    }

    public async Task<IEnumerable<CategoryResponse>> GetCategoriesByTypeAsync(int typeId)
    {
        var categories = await _categoryRepository.GetByTypeIdAsync(typeId);
        return CategoryMapper.ToResponseList(categories);
    }

    public async Task<PaginatedResponse<CategoryResponse>> GetCategoriesByTypePaginatedAsync(int typeId, PaginationRequest request)
    {
        var (data, total) = await _categoryRepository.GetByTypeIdPaginatedAsync(
            typeId,
            request.Q,
            request.Archive,
            request.Skip,
            request.PerPage
        );

        var categoryResponses = CategoryMapper.ToResponseList(data, includeType: true);

        return new PaginatedResponse<CategoryResponse>
        {
            Total = total,
            Data = categoryResponses,
            AllData = categoryResponses,
            Params = request,
            Page = request.Page,
            PerPage = request.PerPage
        };
    }

    public async Task<CategoryResponse?> GetCategoryAsync(int id)
    {
        var category = await _categoryRepository.GetWithTypeAsync(id);
        return category != null ? CategoryMapper.ToResponse(category) : null;
    }

    public async Task<CategoryResponse> CreateCategoryAsync(CategoryResponse categoryDto)
    {
        var category = new Category
        {
            Name = categoryDto.Name,
            TypeId = categoryDto.TypeId,
            ParentCategoryId = categoryDto.ParentCategoryId,
            CreatedAt = DateTime.Now,
            CreatedBy = categoryDto.CreatedBy ?? 1,
            Archived = false
        };
        var created = await _categoryRepository.CreateAsync(category);
        return CategoryMapper.ToResponse(created);
    }

    public async Task<CategoryResponse> UpdateCategoryAsync(int id, UpdateCategoryRequest request)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null) throw new KeyNotFoundException($"Category {id} not found");
        category.Name = request.Name;
        category.ParentCategoryId = request.ParentCategoryId;
        await _categoryRepository.UpdateAsync(category);
        return CategoryMapper.ToResponse(category);
    }

    public async Task ArchiveCategoryAsync(int id)
    {
        var entity = await _categoryRepository.GetByIdAsync(id);
        if (entity == null) throw new KeyNotFoundException($"Category {id} not found");
        await _categoryRepository.ArchiveAsync(id);
    }

    public async Task RestoreCategoryAsync(int id)
    {
        var entity = await _categoryRepository.GetByIdAsync(id);
        if (entity == null) throw new KeyNotFoundException($"Category {id} not found");
        await _categoryRepository.RestoreAsync(id);
    }

    public async Task DeleteCategoryAsync(int id)
    {
        var entity = await _categoryRepository.GetByIdAsync(id);
        if (entity == null) throw new KeyNotFoundException($"Category {id} not found");
        await _categoryRepository.DeleteAsync(id);
    }

    public async Task<PaginatedResponse<CategoryResponse>> GetCategoriesPaginatedAsync(PaginationRequest request)
    {
        var (data, total) = await _categoryRepository.GetPaginatedAsync(
            request.Q,
            request.Archive,
            request.Skip,
            request.PerPage
        );

        var categoryResponses = CategoryMapper.ToResponseList(data, includeType: true);

        return new PaginatedResponse<CategoryResponse>
        {
            Total = total,
            Data = categoryResponses,
            AllData = categoryResponses,
            Params = request,
            Page = request.Page,
            PerPage = request.PerPage
        };
    }
}
