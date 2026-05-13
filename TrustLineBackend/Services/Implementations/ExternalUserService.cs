using AnonymousComplaintsAPI.DTOs.External;
using AnonymousComplaintsAPI.Models.Entities;
using AnonymousComplaintsAPI.Repositories.Interfaces;
using AnonymousComplaintsAPI.Services.Interfaces;

namespace AnonymousComplaintsAPI.Services.Implementations;

public class ExternalUserService : IExternalUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<ExternalUserService> _logger;

    public ExternalUserService(IUserRepository userRepository, ILogger<ExternalUserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<ExternalUserResponse>> GetAllUsersAsync(bool includeArchived = false)
    {
        var users = await _userRepository.GetAllAsync(includeArchived);
        return users.Select(MapToResponse);
    }

    public async Task<ExternalUserResponse?> GetUserByIdAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return user != null ? MapToResponse(user) : null;
    }

    public async Task<ExternalUserResponse> CreateUserAsync(CreateExternalUserRequest request)
    {
        // Check if user already exists
        if (await _userRepository.ExistsAsync(request.UserId))
            throw new InvalidOperationException($"User with ID {request.UserId} already exists");

        var user = new User
        {
            UserId = request.UserId,
            Archive= false
        };

        var created = await _userRepository.CreateAsync(user);
        _logger.LogInformation("External API: Created user {UserId}", created.UserId);
        return MapToResponse(created);
    }

    public async Task<ExternalUserResponse?> UpdateUserAsync(int userId, UpdateExternalUserRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return null;

        if (request.Archive.HasValue)
            user.Archive = request.Archive.Value;

        await _userRepository.UpdateAsync(user);
        _logger.LogInformation("External API: Updated user {UserId}", userId);
        return MapToResponse(user);
    }

    public async Task<bool> UserExistsAsync(int userId)
    {
        return await _userRepository.ExistsAsync(userId);
    }

    public async Task ArchiveUserAsync(int userId)
    {
        await _userRepository.ArchiveAsync(userId);
        _logger.LogInformation("External API: Archived user {UserId}", userId);
    }

    public async Task RestoreUserAsync(int userId)
    {
        await _userRepository.RestoreAsync(userId);
        _logger.LogInformation("External API: Restored user {UserId}", userId);
    }

    private static ExternalUserResponse MapToResponse(User user)
    {
        return new ExternalUserResponse
        {
            UserId = user.UserId,
            Archive = user.Archive
        };
    }
}
