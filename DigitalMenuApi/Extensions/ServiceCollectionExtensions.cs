using DigitalMenuApi.Data;
using Microsoft.EntityFrameworkCore;

namespace DigitalMenuApi.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all application services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Controllers
        services.AddControllers();

        // Add API Explorer for Swagger
        services.AddEndpointsApiExplorer();

        // Add Swagger/OpenAPI
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Digital Menu API",
                Version = "v1",
                Description = "API for Digital Menu with Nutrition Labelling"
            });
        });

        // Add DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // TODO: Add other services here as you build them:
        // - Repositories
        // - Application Services
        // - JWT Authentication
        // - AWS S3 Storage Service
        // - AutoMapper

        return services;
    }
}
