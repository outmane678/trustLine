using AnonymousComplaintsAPI.DTOs.Responses;

namespace AnonymousComplaintsAPI.Services.Interfaces;

/// <summary>
/// Service for managing complaint types with cascading operations
/// </summary>
public interface ITypeService
{
    /// <summary>
    /// Gets all types, optionally including archived and categories
    /// </summary>
    Task<IEnumerable<TypeModelResponse>> GetAllTypesAsync(bool includeArchived = false, bool includeCategories = true);

    /// <summary>
    /// Gets a single type by ID
    /// </summary>
    Task<TypeModelResponse?> GetTypeAsync(int id, bool includeCategories = true);

    /// <summary>
    /// Creates a new type
    /// </summary>
    Task<TypeModelResponse> CreateTypeAsync(TypeModelResponse typeDto);

    /// <summary>
    /// Updates an existing type
    /// </summary>
    Task<TypeModelResponse> UpdateTypeAsync(int id, TypeModelResponse typeDto);

    /// <summary>
    /// Archives a type and all its related categories (cascading soft delete)
    /// </summary>
    Task ArchiveTypeWithCategoriesAsync(int id);

    /// <summary>
    /// Restores a type and all its related categories (cascading restore)
    /// </summary>
    Task RestoreTypeWithCategoriesAsync(int id);

    /// <summary>
    /// Permanently deletes a type
    /// </summary>
    Task DeleteTypeAsync(int id);

    /// <summary>
    /// Gets types with pagination and filtering
    /// </summary>
    Task<DTOs.Responses.PaginatedResponse<TypeModelResponse>> GetTypesPaginatedAsync(DTOs.Requests.PaginationRequest request);
}
