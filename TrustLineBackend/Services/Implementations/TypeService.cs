using AnonymousComplaintsAPI.DTOs.Responses;
using AnonymousComplaintsAPI.Mappers;
using AnonymousComplaintsAPI.Models.Entities;
using AnonymousComplaintsAPI.Repositories.Interfaces;
using AnonymousComplaintsAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using AnonymousComplaintsAPI.Data;

namespace AnonymousComplaintsAPI.Services.Implementations;

public class TypeService : ITypeService
{
    private readonly ITypeRepository _typeRepository;
    private readonly AnonymousComplaintsV002Context _context;
    private readonly ILogger<TypeService> _logger;

    public TypeService(ITypeRepository typeRepository, AnonymousComplaintsV002Context context, ILogger<TypeService> logger)
    {
        _typeRepository = typeRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<TypeModelResponse>> GetAllTypesAsync(bool includeArchived = false, bool includeCategories = true)
    {
        IEnumerable<AnonymousComplaintsAPI.Models.Entities.Type> types;
        types = includeCategories
            ? (includeArchived ? await _typeRepository.GetAllWithCategoriesAsync() : await _typeRepository.GetNonArchivedWithCategoriesAsync())
            : (includeArchived ? await _typeRepository.GetAllAsync() : await _typeRepository.GetNonArchivedAsync());
        return TypeModelMapper.ToResponseList(types);
    }

    public async Task<TypeModelResponse?> GetTypeAsync(int id, bool includeCategories = true)
    {
        var type = includeCategories ? await _typeRepository.GetWithCategoriesAsync(id) : await _typeRepository.GetByIdAsync(id);
        return type != null ? TypeModelMapper.ToResponse(type) : null;
    }

    public async Task<TypeModelResponse> CreateTypeAsync(TypeModelResponse typeDto)
    {
        var type = new AnonymousComplaintsAPI.Models.Entities.Type
        {
            Name = typeDto.Name,
            CreatedAt = DateTime.Now,
            CreatedBy = typeDto.CreatedBy ?? 1,
            Archived = false
        };
        var created = await _typeRepository.CreateAsync(type);
        return TypeModelMapper.ToResponse(created);
    }

    public async Task<TypeModelResponse> UpdateTypeAsync(int id, TypeModelResponse typeDto)
    {
        var type = await _typeRepository.GetByIdAsync(id);
        if (type == null) throw new KeyNotFoundException($"Type {id} not found");
        type.Name = typeDto.Name;
        await _typeRepository.UpdateAsync(type);
        return TypeModelMapper.ToResponse(type);
    }

    public async Task ArchiveTypeWithCategoriesAsync(int id)
    {
        var type = await _typeRepository.GetByIdAsync(id);
        if (type == null) throw new KeyNotFoundException($"Type {id} not found");
        await _typeRepository.ArchiveAsync(id);
        var categories = await _context.Categories.Where(c => c.TypeId == id).ToListAsync();
        foreach (var c in categories) c.Archived = true;
        await _context.SaveChangesAsync();
    }

    public async Task RestoreTypeWithCategoriesAsync(int id)
    {
        var type = await _typeRepository.GetByIdAsync(id);
        if (type == null) throw new KeyNotFoundException($"Type {id} not found");
        await _typeRepository.RestoreAsync(id);
        var categories = await _context.Categories.Where(c => c.TypeId == id).ToListAsync();
        foreach (var c in categories) c.Archived = false;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteTypeAsync(int id) => await _typeRepository.DeleteAsync(id);

    public async Task<DTOs.Responses.PaginatedResponse<TypeModelResponse>> GetTypesPaginatedAsync(DTOs.Requests.PaginationRequest request)
    {
        var (data, total) = await _typeRepository.GetPaginatedAsync(
            request.Q,
            request.Archive,
            request.Skip,
            request.PerPage
        );

        var typeResponses = TypeModelMapper.ToResponseList(data);

        return new DTOs.Responses.PaginatedResponse<TypeModelResponse>
        {
            Total = total,
            Data = typeResponses,
            AllData = typeResponses,
            Params = request,
            Page = request.Page,
            PerPage = request.PerPage
        };
    }
}
