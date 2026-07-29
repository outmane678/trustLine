using AnonymousComplaintsAPI.DTOs.Responses;
using AnonymousComplaintsAPI.Mappers;
using AnonymousComplaintsAPI.Models.Entities;
using AnonymousComplaintsAPI.Repositories.Interfaces;
using AnonymousComplaintsAPI.Services.Interfaces;

namespace AnonymousComplaintsAPI.Services.Implementations;

public class FrequencyService : IFrequencyService
{
    private readonly IFrequencyRepository _frequencyRepository;
    private readonly ILogger<FrequencyService> _logger;

    public FrequencyService(IFrequencyRepository frequencyRepository, ILogger<FrequencyService> logger)
    {
        _frequencyRepository = frequencyRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<FrequencyResponse>> GetAllFrequenciesAsync(bool includeArchived = false)
    {
        var frequencies = includeArchived ? await _frequencyRepository.GetAllAsync() : await _frequencyRepository.GetNonArchivedAsync();
        return FrequencyMapper.ToResponseList(frequencies);
    }

    public async Task<FrequencyResponse?> GetFrequencyAsync(int id)
    {
        var frequency = await _frequencyRepository.GetByIdAsync(id);
        return frequency != null ? FrequencyMapper.ToResponse(frequency) : null;
    }

    public async Task<FrequencyResponse> CreateFrequencyAsync(FrequencyResponse frequencyDto)
    {
        var frequency = new Frequency
        {
            Label = frequencyDto.Label,
            CreatedAt = DateTime.Now,
            CreatedBy = frequencyDto.CreatedBy ?? 1,
            Archived = false
        };
        var created = await _frequencyRepository.CreateAsync(frequency);
        return FrequencyMapper.ToResponse(created);
    }

    public async Task<FrequencyResponse> UpdateFrequencyAsync(int id, FrequencyResponse frequencyDto)
    {
        var frequency = await _frequencyRepository.GetByIdAsync(id);
        if (frequency == null) throw new KeyNotFoundException($"Frequency {id} not found");
        frequency.Label = frequencyDto.Label;
        await _frequencyRepository.UpdateAsync(frequency);
        return FrequencyMapper.ToResponse(frequency);
    }

    public async Task ArchiveFrequencyAsync(int id)
    {
        var entity = await _frequencyRepository.GetByIdAsync(id);
        if (entity == null) throw new KeyNotFoundException($"Frequency {id} not found");
        await _frequencyRepository.ArchiveAsync(id);
    }

    public async Task RestoreFrequencyAsync(int id)
    {
        var entity = await _frequencyRepository.GetByIdAsync(id);
        if (entity == null) throw new KeyNotFoundException($"Frequency {id} not found");
        await _frequencyRepository.RestoreAsync(id);
    }

    public async Task DeleteFrequencyAsync(int id)
    {
        var entity = await _frequencyRepository.GetByIdAsync(id);
        if (entity == null) throw new KeyNotFoundException($"Frequency {id} not found");
        await _frequencyRepository.DeleteAsync(id);
    }
}
