namespace AnonymousComplaintsAPI.Configurations
{
    /// <summary>
    /// Configuration for Cross-Origin Resource Sharing (CORS) policies
    /// </summary>
    public static class CorsConfiguration
    {
        /// <summary>
        /// Adds CORS services with a permissive default policy
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                // Default policy: Allow all origins, methods, and headers
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .SetIsOriginAllowedToAllowWildcardSubdomains();
                });

                // Named policy example (currently commented out in Program.cs)
                // Uncomment and configure if specific origin restrictions are needed
                /*
                options.AddPolicy("AllowLocalhost3000", builder =>
                {
                    builder.WithOrigins("http://localhost:3000")
                           .AllowAnyHeader()
                           .AllowAnyMethod()
                           .SetIsOriginAllowedToAllowWildcardSubdomains();
                });
                */
            });

            return services;
        }
    }
}
