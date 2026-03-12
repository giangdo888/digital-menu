namespace DigitalMenuApi.Extensions;
using DigitalMenuApi.Middleware;

public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Configures the HTTP request pipeline
    /// </summary>
    public static WebApplication ConfigureApplicationPipeline(this WebApplication app)
    {

        // add exception middleware
        app.UseMiddleware<ExceptionMiddleware>();
        // Configure Swagger in Development
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Digital Menu API v1");
                options.RoutePrefix = "swagger"; // Swagger at /swagger
            });
        }

        // Security & Routing
        app.UseHttpsRedirection();
        app.UseCors("AllowFrontend");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        // TODO: Add middleware here as needed:
        // - CORS
        // - Exception Handling Middleware
        // - Authentication Middleware
        // - Request Logging

        return app;
    }
}
