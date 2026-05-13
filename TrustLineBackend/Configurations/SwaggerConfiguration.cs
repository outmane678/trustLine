using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AnonymousComplaintsAPI.Configurations
{
    /// <summary>
    /// Configuration for Swagger/OpenAPI documentation and JWT authentication in Swagger UI
    /// </summary>
    public static class SwaggerConfiguration
    {
        /// <summary>
        /// Adds Swagger generation services with JWT Bearer authentication support
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen(options =>
            {
                // Add JWT Bearer authentication to Swagger
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Collez uniquement le token JWT (sans le préfixe Bearer)."
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });

                // Add file upload support filter
                options.OperationFilter<AddFileUploadParamOperationFilter>();
            });

            return services;
        }

        /// <summary>
        /// Configures Swagger middleware in the application pipeline
        /// </summary>
        /// <param name="app">The web application</param>
        /// <returns>The web application for chaining</returns>
        public static WebApplication UseSwaggerConfiguration(this WebApplication app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "AnonymousComplaintsAPI V1.0.0");
                c.RoutePrefix = string.Empty; // Set Swagger UI as the default route
            });

            return app;
        }
    }
}
