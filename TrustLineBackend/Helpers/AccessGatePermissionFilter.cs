using AnonymousComplaintsAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AnonymousComplaintsAPI.Helpers;

/// <summary>
/// Custom attribute to require an AccessGate permission on an endpoint.
/// Usage: [RequirePermission("tl-v-report")]
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequirePermissionAttribute : TypeFilterAttribute
{
    public RequirePermissionAttribute(string permission)
        : base(typeof(AccessGatePermissionFilter))
    {
        Arguments = new object[] { permission };
    }
}

/// <summary>
/// Action filter that checks user permissions via the AccessGate external API.
/// Extracts the user ID from the JWT claims and calls AccessGateService.CheckPermissionAsync.
/// Returns 403 Forbidden if the user does not have the required permission.
/// </summary>
public class AccessGatePermissionFilter : IAsyncActionFilter
{
    private readonly IAccessGateService _accessGateService;
    private readonly string _permission;
    private readonly ILogger<AccessGatePermissionFilter> _logger;

    public AccessGatePermissionFilter(
        IAccessGateService accessGateService,
        string permission,
        ILogger<AccessGatePermissionFilter> logger)
    {
        _accessGateService = accessGateService;
        _permission = permission;
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Extract user ID from JWT claims
        var userIdClaim = context.HttpContext.User.FindFirst("Id");

        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            _logger.LogWarning("Permission check failed: cannot resolve user ID from token for permission '{Permission}'", _permission);
            context.Result = new ObjectResult(new { error = "Cannot resolve user identity from token" })
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
            return;
        }

        var hasPermission = await _accessGateService.CheckPermissionAsync(userId, _permission);

        if (!hasPermission)
        {
            _logger.LogWarning("Permission denied: userId={UserId} does not have permission '{Permission}'", userId, _permission);
            context.Result = new ObjectResult(new { error = $"Access denied. Required permission: {_permission}" })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
            return;
        }

        await next();
    }
}
