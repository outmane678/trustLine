namespace AnonymousComplaintsAPI.DTOs.External;

// ─────────────────────────────────────────────────────────────────────────
// Responses
// ─────────────────────────────────────────────────────────────────────────

/// <summary>
/// User returned by the external API
/// </summary>
public class ExternalUserResponse
{
    public int UserId { get; set; }
    public bool Archive { get; set; }
}

// ─────────────────────────────────────────────────────────────────────────
// Requests
// ─────────────────────────────────────────────────────────────────────────

/// <summary>
/// Request to create a user via the external API
/// </summary>
public class CreateExternalUserRequest
{
    /// <summary>
    /// The AccessGate user ID (must match the ID in AccessGate)
    /// </summary>
    public int UserId { get; set; }
}

/// <summary>
/// Request to update a user via the external API
/// </summary>
public class UpdateExternalUserRequest
{
    /// <summary>
    /// Set to true to archive, false to unarchive. Omit (null) to leave unchanged.
    /// </summary>
    public bool? Archive { get; set; }
}
