using AnonymousComplaintsAPI.DTOs.Requests;
using AnonymousComplaintsAPI.DTOs.Responses;
using AnonymousComplaintsAPI.Mappers;
using AnonymousComplaintsAPI.Models.Entities;
using AnonymousComplaintsAPI.Repositories.Interfaces;
using AnonymousComplaintsAPI.Services.Interfaces;

namespace AnonymousComplaintsAPI.Services.Implementations;

public class SolutionService : ISolutionService
{
    private readonly ISolutionRepository _solutionRepository;
    private readonly IAnonymousComplaintRepository _complaintRepository;
    private readonly ILogger<SolutionService> _logger;

    public SolutionService(ISolutionRepository solutionRepository, IAnonymousComplaintRepository complaintRepository, ILogger<SolutionService> logger)
    {
        _solutionRepository = solutionRepository;
        _complaintRepository = complaintRepository;
        _logger = logger;
    }

    public async Task<SolutionResponse?> GetSolutionAsync(int id)
    {
        var solution = await _solutionRepository.GetByIdAsync(id);
        return solution != null ? SolutionMapper.ToResponse(solution,null) : null;
    }

    public async Task<IEnumerable<SolutionResponse>> GetAllSolutionsAsync()
    {
        var solutions = await _solutionRepository.GetNonArchivedAsync();
        return SolutionMapper.ToResponseList(solutions,null);
    }

    public async Task<IEnumerable<SolutionResponse>> GetAllSolutionsIncludingArchivedAsync()
    {
        var solutions = await _solutionRepository.GetAllAsync();
        return SolutionMapper.ToResponseList(solutions,null);
    }

    public async Task<IEnumerable<SolutionResponse>> GetSolutionsByComplaintAsync(int complaintId)
    {
        var solutions = await _solutionRepository.GetByComplaintIdAsync(complaintId);
        return SolutionMapper.ToResponseList(solutions,null);
    }

    public async Task<SolutionResponse> CreateSolutionAsync(SendResponseRequest request)
    {
        if (!request.AnonymousComplaintID.HasValue) throw new ArgumentException("AnonymousComplaintID is required");
        var solution = new Solution
        {
            Content = request.Content,
            AnonymousComplaintId = request.AnonymousComplaintID.Value,
            CreatedBy = request.CreatedBy ?? 1,
            CreatedAt = DateTime.Now,
            Archived = false
        };
        var created = await _solutionRepository.CreateAsync(solution);
        await _complaintRepository.UpdateStateAsync(request.AnonymousComplaintID.Value, "RESOLVED");
        return SolutionMapper.ToResponse(created,null);
    }

    public async Task<SolutionResponse> CreateSolutionForComplaintAndMergedAsync(SendResponseRequest request)
    {
        if (!request.AnonymousComplaintID.HasValue) throw new ArgumentException("AnonymousComplaintID is required");
        var mainSolution = new Solution
        {
            Content = request.Content,
            AnonymousComplaintId = request.AnonymousComplaintID.Value,
            CreatedBy = request.CreatedBy ?? 1,
            CreatedAt = DateTime.Now,
            Archived = false
        };
        var created = await _solutionRepository.CreateAsync(mainSolution);
        await _complaintRepository.UpdateStateAsync(request.AnonymousComplaintID.Value, "RESOLVED");
        
        var merged = await _complaintRepository.GetMergedComplaintsAsync(request.AnonymousComplaintID.Value);
        foreach (var m in merged)
        {
            await _solutionRepository.CreateAsync(new Solution
            {
                Content = request.Content,
                AnonymousComplaintId = m.AnonymousComplaintId,
                CreatedBy = request.CreatedBy ?? 1,
                CreatedAt = DateTime.Now,
                Archived = false
            });
        }
        var ids = merged.Select(c => c.AnonymousComplaintId).ToList();
        if (ids.Any())
            await _complaintRepository.UpdateMultipleStatesAsync(ids, "RESOLVED");
        
        return SolutionMapper.ToResponse(created,null);
    }

    public async Task<SolutionResponse> UpdateSolutionAsync(int id, SendResponseRequest request)
    {
        var solution = await _solutionRepository.GetByIdAsync(id);
        if (solution == null) throw new KeyNotFoundException($"Solution {id} not found");
        solution.Content = request.Content;
        await _solutionRepository.UpdateAsync(solution);
        return SolutionMapper.ToResponse(solution,null);
    }

    public async Task ArchiveSolutionAsync(int id) => await _solutionRepository.ArchiveAsync(id);
    public async Task RestoreSolutionAsync(int id) => await _solutionRepository.RestoreAsync(id);
    public async Task DeleteSolutionAsync(int id) => await _solutionRepository.DeleteAsync(id);
}
