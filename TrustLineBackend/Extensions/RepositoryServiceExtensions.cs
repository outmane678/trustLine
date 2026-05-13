using AnonymousComplaintsAPI.Repositories.Implementations;
using AnonymousComplaintsAPI.Repositories.Interfaces;

namespace AnonymousComplaintsAPI.Extensions
{
    public static class RepositoryServiceExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            // Register all repositories
            services.AddScoped<IAnonymousComplaintRepository, AnonymousComplaintRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IAttachmentRepository, AttachmentRepository>();
            services.AddScoped<ISolutionRepository, SolutionRepository>();
            services.AddScoped<ITypeRepository, TypeRepository>();
            services.AddScoped<IFrequencyRepository, FrequencyRepository>();

            return services;
        }
    }
}
