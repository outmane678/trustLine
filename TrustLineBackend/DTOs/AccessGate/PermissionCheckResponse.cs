namespace AnonymousComplaintsAPI.DTOs.AccessGate;

/// <summary>
/// Response from AccessGate's /api/external/permissions/check endpoint
/// </summary>
public class PermissionCheckResponse
{
    public int UserId { get; set; }
    public string Permission { get; set; } = string.Empty;
    public bool HasPermission { get; set; }
    public int? AppId { get; set; }
    public int? ProjectId { get; set; }
}
