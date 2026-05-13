using AnonymousComplaintsAPI.DTOs.External;
using AnonymousComplaintsAPI.Helpers;
using AnonymousComplaintsAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnonymousComplaintsAPI.Controllers;

/// <summary>
/// External service endpoints for user CRUD operations.
/// Accessible by other applications (AccessGate, HrLink, etc.) using a service token.
/// 
/// All endpoints require:
/// 1. A valid JWT Bearer token (validated by [Authorize])
/// 2. The appropriate tl-ext-* permission in the token's role claims
/// 
/// Permissions:
/// - tl-ext-read-users     → GET endpoints (read user data)
/// - tl-ext-create-users   → POST endpoint (create new users)
/// - tl-ext-update-users   → PUT endpoint (update user data)
/// - tl-ext-archive-users  → PATCH archive (soft-delete)
/// - tl-ext-restore-users  → PATCH restore (re-enable)
/// </summary>
[Route("api/external")]
[ApiController]
[Authorize]
public class ExternalUserController : ControllerBase
{
    private readonly IExternalUserService _externalUserService;
    private readonly ILogger<ExternalUserController> _logger;

    public ExternalUserController(
        IExternalUserService externalUserService,
        ILogger<ExternalUserController> logger)
    {
        _externalUserService = externalUserService;
        _logger = logger;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GET — Read Users
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Get all users
    /// </summary>
    /// <remarks>
    /// Returns all users in the TrustLine database.
    /// Use ?includeArchived=true to include archived users.
    ///
    /// Examples:
    /// - GET /api/external/users
    /// - GET /api/external/users?includeArchived=true
    /// </remarks>
    /// <param name="includeArchived">Include archived users (default: false)</param>
    [HttpGet("users")]
    [RequireExternalPermission("tl-ext-read-users")]
    [ProducesResponseType(typeof(IEnumerable<ExternalUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllUsers([FromQuery] bool includeArchived = false)
    {
        try
        {
            var users = await _externalUserService.GetAllUsersAsync(includeArchived);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[External API] Error fetching users");
            return StatusCode(500, new { error = "An error occurred while fetching users" });
        }
    }

    /// <summary>
    /// Get a user by ID
    /// </summary>
    /// <remarks>
    /// Returns user details for the given UserId.
    ///
    /// Example: GET /api/external/users/42
    /// </remarks>
    /// <param name="userId">User ID</param>
    [HttpGet("users/{userId:int}")]
    [RequireExternalPermission("tl-ext-read-users")]
    [ProducesResponseType(typeof(ExternalUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(int userId)
    {
        try
        {
            var user = await _externalUserService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound(new { error = $"User {userId} not found" });

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[External API] Error fetching user {UserId}", userId);
            return StatusCode(500, new { error = "An error occurred while fetching the user" });
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // POST — Create User
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Create a new user
    /// </summary>
    /// <remarks>
    /// Creates a new user in the TrustLine database. The UserId must match the AccessGate user ID.
    ///
    /// Example request:
    /// ```json
    /// { "userId": 42 }
    /// ```
    /// </remarks>
    /// <param name="request">User creation request</param>
    [HttpPost("users")]
    [RequireExternalPermission("tl-ext-create-users")]
    [ProducesResponseType(typeof(ExternalUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateUser([FromBody] CreateExternalUserRequest request)
    {
        if (request.UserId <= 0)
            return BadRequest(new { error = "UserId must be a positive integer" });

        try
        {
            var user = await _externalUserService.CreateUserAsync(request);
            return CreatedAtAction(nameof(GetUserById), new { userId = user.UserId }, user);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[External API] Error creating user {UserId}", request.UserId);
            return StatusCode(500, new { error = "An error occurred while creating the user" });
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PUT — Update User
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Update a user
    /// </summary>
    /// <remarks>
    /// Applies a partial update to a user. Only non-null fields are changed.
    ///
    /// Example request:
    /// ```json
    /// { "archived": true }
    /// ```
    /// </remarks>
    /// <param name="userId">User ID</param>
    /// <param name="request">Update request</param>
    [HttpPut("users/{userId:int}")]
    [RequireExternalPermission("tl-ext-update-users")]
    [ProducesResponseType(typeof(ExternalUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser(int userId, [FromBody] UpdateExternalUserRequest request)
    {
        try
        {
            var user = await _externalUserService.UpdateUserAsync(userId, request);
            if (user == null)
                return NotFound(new { error = $"User {userId} not found" });

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[External API] Error updating user {UserId}", userId);
            return StatusCode(500, new { error = "An error occurred while updating the user" });
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PATCH — Archive / Restore
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Archive a user (soft delete)
    /// </summary>
    /// <remarks>
    /// Sets the Archived flag to true. The user record is retained but excluded from active listings.
    /// Reversed by the restore endpoint.
    ///
    /// Example: PATCH /api/external/users/42/archive
    /// </remarks>
    /// <param name="userId">User ID</param>
    [HttpPatch("users/{userId:int}/archive")]
    [RequireExternalPermission("tl-ext-archive-users")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ArchiveUser(int userId)
    {
        try
        {
            var user = await _externalUserService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound(new { error = $"User {userId} not found" });

            if (user.Archive)
                return Conflict(new { error = $"User {userId} is already archived" });

            await _externalUserService.ArchiveUserAsync(userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[External API] Error archiving user {UserId}", userId);
            return StatusCode(500, new { error = "An error occurred while archiving the user" });
        }
    }

    /// <summary>
    /// Restore an archived user
    /// </summary>
    /// <remarks>
    /// Sets the Archived flag to false. Re-enables the user.
    ///
    /// Example: PATCH /api/external/users/42/restore
    /// </remarks>
    /// <param name="userId">User ID</param>
    [HttpPatch("users/{userId:int}/restore")]
    [RequireExternalPermission("tl-ext-restore-users")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RestoreUser(int userId)
    {
        try
        {
            var user = await _externalUserService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound(new { error = $"User {userId} not found" });

            if (!user.Archive)
                return Conflict(new { error = $"User {userId} is not archived" });

            await _externalUserService.RestoreUserAsync(userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[External API] Error restoring user {UserId}", userId);
            return StatusCode(500, new { error = "An error occurred while restoring the user" });
        }
    }
}
