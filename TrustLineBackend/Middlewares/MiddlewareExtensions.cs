namespace AnonymousComplaintsAPI.Middlewares
{
    /// <summary>
    /// Extension methods for registering custom middleware
    /// </summary>
    public static class MiddlewareExtensions
    {
        /// <summary>
        /// Adds global error handling middleware to the application pipeline
        /// </summary>
        public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ErrorHandlingMiddleware>();
        }

        /// <summary>
        /// Adds request/response logging middleware to the application pipeline
        /// </summary>
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RequestLoggingMiddleware>();
        }
    }
}
