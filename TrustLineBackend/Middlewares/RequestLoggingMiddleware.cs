using System.Diagnostics;
using System.Text;

namespace AnonymousComplaintsAPI.Middlewares
{
    /// <summary>
    /// Middleware for logging HTTP requests and responses with performance metrics
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip logging for static files and health checks
            if (IsStaticFileOrHealthCheck(context.Request.Path))
            {
                await _next(context);
                return;
            }

            var requestId = Guid.NewGuid().ToString();
            context.Items["RequestId"] = requestId;

            var stopwatch = Stopwatch.StartNew();

            // Log request
            await LogRequest(context, requestId);

            // Capture the original response body stream
            var originalBodyStream = context.Response.Body;

            try
            {
                using var responseBody = new MemoryStream();
                context.Response.Body = responseBody;

                // Execute the next middleware
                await _next(context);

                stopwatch.Stop();

                // Log response
                await LogResponse(context, requestId, stopwatch.ElapsedMilliseconds);

                // Reset the position to the beginning before copying
                responseBody.Seek(0, SeekOrigin.Begin);

                // Copy response body back to original stream
                await responseBody.CopyToAsync(originalBodyStream);
            }
            finally
            {
                context.Response.Body = originalBodyStream;
                stopwatch.Stop();
            }
        }

        private async Task LogRequest(HttpContext context, string requestId)
        {
            var request = context.Request;

            var logMessage = new StringBuilder();
            logMessage.AppendLine($"[{requestId}] HTTP Request");
            logMessage.AppendLine($"Method: {request.Method}");
            logMessage.AppendLine($"Path: {request.Path}{request.QueryString}");
            logMessage.AppendLine($"Remote IP: {context.Connection.RemoteIpAddress}");
            logMessage.AppendLine($"User Agent: {request.Headers["User-Agent"]}");

            // Log request body for non-GET requests (excluding file uploads)
            if (request.Method != "GET" && request.ContentLength > 0 && request.ContentLength < 10000)
            {
                request.EnableBuffering();
                var buffer = new byte[Convert.ToInt32(request.ContentLength)];
                await request.Body.ReadAsync(buffer, 0, buffer.Length);
                var bodyAsText = Encoding.UTF8.GetString(buffer);
                logMessage.AppendLine($"Body: {bodyAsText}");
                request.Body.Position = 0;
            }

            _logger.LogInformation(logMessage.ToString());
        }

        private Task LogResponse(HttpContext context, string requestId, long elapsedMilliseconds)
        {
            var response = context.Response;

            var logMessage = new StringBuilder();
            logMessage.AppendLine($"[{requestId}] HTTP Response");
            logMessage.AppendLine($"Status Code: {response.StatusCode}");
            logMessage.AppendLine($"Duration: {elapsedMilliseconds}ms");

            var logLevel = response.StatusCode >= 500
                ? LogLevel.Error
                : response.StatusCode >= 400
                    ? LogLevel.Warning
                    : LogLevel.Information;

            _logger.Log(logLevel, logMessage.ToString());

            return Task.CompletedTask;
        }

        private static bool IsStaticFileOrHealthCheck(PathString path)
        {
            var pathValue = path.Value?.ToLower() ?? string.Empty;

            return pathValue.StartsWith("/wwwroot") ||
                   pathValue.StartsWith("/swagger") ||
                   pathValue.Contains("/health") ||
                   pathValue.EndsWith(".css") ||
                   pathValue.EndsWith(".js") ||
                   pathValue.EndsWith(".map") ||
                   pathValue.EndsWith(".ico") ||
                   pathValue.EndsWith(".png") ||
                   pathValue.EndsWith(".jpg") ||
                   pathValue.EndsWith(".jpeg") ||
                   pathValue.EndsWith(".gif") ||
                   pathValue.EndsWith(".svg");
        }
    }
}
