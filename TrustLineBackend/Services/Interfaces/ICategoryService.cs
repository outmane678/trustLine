using AnonymousComplaintsAPI.DTOs.Requests;
using AnonymousComplaintsAPI.DTOs.Responses;

namespace AnonymousComplaintsAPI.Services.Interfaces;

/// <summary>
/// Service for managing complaint categories
/// </summary>
public interface ICategoryService
{
    /// <summary>
    /// Gets all categories, optionally including archived ones
    /// </summary>
    Task<IEnumerable<CategoryResponse>> GetAllCategoriesAsync(bool includeArchived = false);

    /// <summary>
    /// Gets all categories for a specific type
    /// </summary>
    Task<IEnumerable<CategoryResponse>> GetCategoriesByTypeAsync(int typeId);

    /// <summary>
    /// Gets categories for a specific type with pagination and filtering
    /// </summary>
    Task<PaginatedResponse<CategoryResponse>> GetCategoriesByTypePaginatedAsync(int typeId, PaginationRequest request);

    /// <summary>
    /// Gets a single category by ID
    /// </summary>
    Task<CategoryResponse?> GetCategoryAsync(int id);

    /// <summary>
    /// Creates a new category
    /// </summary>
    Task<CategoryResponse> CreateCategoryAsync(CategoryResponse categoryDto);

    /// <summary>
    /// Updates an existing category
    /// </summary>
    Task<CategoryResponse> UpdateCategoryAsync(int id, UpdateCategoryRequest request);

    /// <summary>
    /// Archives a category (soft delete)
    /// </summary>
    Task ArchiveCategoryAsync(int id);

    /// <summary>
    /// Restores an archived category
    /// </summary>
    Task RestoreCategoryAsync(int id);

    /// <summary>
    /// Permanently deletes a category
    /// </summary>
    Task DeleteCategoryAsync(int id);

    /// <summary>
    /// Gets categories with pagination and filtering
    /// </summary>
    Task<PaginatedResponse<CategoryResponse>> GetCategoriesPaginatedAsync(PaginationRequest request);
}
