using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using AnonymousComplaintsAPI.Models;
using System.Text;

namespace AnonymousComplaintsAPI.Configurations
{
    /// <summary>
    /// Configuration for JWT authentication and authorization
    /// </summary>
    public static class JwtConfiguration
    {
        /// <summary>
        /// Adds JWT authentication services with configuration from appsettings
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The application configuration</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddJwtConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            // Bind JWT settings from configuration
            var jwtSettings = new JwtSettings();
            configuration.GetSection("JsonWebTokenKeys").Bind(jwtSettings);
            services.AddSingleton(jwtSettings);

            // Configure JWT authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = jwtSettings.ValidateIssuerSigningKey,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.IssuerSigningKey)),
                    ValidateIssuer = jwtSettings.ValidateIssuer,
                    ValidIssuer = jwtSettings.ValidIssuer,
                    ValidateAudience = jwtSettings.ValidateAudience,
                    ValidAudience = jwtSettings.ValidAudience,
                    RequireExpirationTime = jwtSettings.RequireExpirationTime,
                    ValidateLifetime = jwtSettings.RequireExpirationTime,
                    ClockSkew = TimeSpan.FromDays(3)
                };
            });

            return services;
        }
    }
}
