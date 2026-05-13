using Microsoft.EntityFrameworkCore;
using AnonymousComplaintsAPI.Models.Entities;
using AnonymousComplaintsAPI.Data;


namespace AnonymousComplaintsAPI.Configurations
{
    /// <summary>
    /// Configuration for database context and Entity Framework Core
    /// </summary>
    public static class DatabaseConfiguration
    {
        /// <summary>
        /// Adds database context services with SQL Server configuration
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The application configuration</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AnonymousComplaintsV002Context>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sqlServerOptions => sqlServerOptions
                        .EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null)
                        .CommandTimeout(60)));

            return services;
        }
    }
}
