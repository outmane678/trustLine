using AnonymousComplaintsAPI.Repositories.Implementations;
using AnonymousComplaintsAPI.Repositories.Interfaces;
using AnonymousComplaintsAPI.Services.Implementations;
using AnonymousComplaintsAPI.Services.Interfaces;

namespace AnonymousComplaintsAPI.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
         // Core business services
        services.AddScoped<IAnonymousComplaintService, AnonymousComplaintService>();
        services.AddScoped<ISolutionService, SolutionService>();
        services.AddScoped<IAttachmentService, AttachmentService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ITypeService, TypeService>();
        services.AddScoped<IFrequencyService, FrequencyService>();

        // Infrastructure services
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IEmailService, EmailService>();

        // External API services
        services.AddScoped<IHrLinkService, HrLinkService>();
        services.AddScoped<IAccessGateService, AccessGateService>();

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();

        // External user service (for ExternalUserController)
        services.AddScoped<IExternalUserService, ExternalUserService>();

        return services;
    }
}
