using System.Net;
using System.Text.Json;

namespace AnonymousComplaintsAPI.Middlewares
{
    /// <summary>
    /// Middleware for global exception handling and standardized error responses
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;

        public ErrorHandlingMiddleware(
            RequestDelegate next,
            ILogger<ErrorHandlingMiddleware> logger,
            IWebHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var statusCode = exception switch
            {
                ArgumentNullException => HttpStatusCode.BadRequest,
                ArgumentException => HttpStatusCode.BadRequest,
                KeyNotFoundException => HttpStatusCode.NotFound,
                UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                InvalidOperationException => HttpStatusCode.BadRequest,
                _ => HttpStatusCode.InternalServerError
            };

            context.Response.StatusCode = (int)statusCode;

            var response = new ErrorResponse
            {
                StatusCode = (int)statusCode,
                Message = GetUserFriendlyMessage(statusCode),
                Details = _environment.IsDevelopment() ? exception.Message : null,
                StackTrace = _environment.IsDevelopment() ? exception.StackTrace : null,
                Timestamp = DateTime.UtcNow
            };

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            await context.Response.WriteAsJsonAsync(response, jsonOptions);
        }

        private static string GetUserFriendlyMessage(HttpStatusCode statusCode)
        {
            return statusCode switch
            {
                HttpStatusCode.BadRequest => "The request was invalid or cannot be processed.",
                HttpStatusCode.NotFound => "The requested resource was not found.",
                HttpStatusCode.Unauthorized => "You are not authorized to access this resource.",
                HttpStatusCode.InternalServerError => "An error occurred while processing your request.",
                _ => "An unexpected error occurred."
            };
        }
    }

    /// <summary>
    /// Standardized error response model
    /// </summary>
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; }
        public string? StackTrace { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
