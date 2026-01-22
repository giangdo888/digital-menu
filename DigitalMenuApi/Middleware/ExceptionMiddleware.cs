namespace DigitalMenuApi.Middleware;

using System.Net;
  using System.Text.Json;

  public class ExceptionMiddleware
  {
      private readonly RequestDelegate _next;
      private readonly ILogger<ExceptionMiddleware> _logger;

      public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
      {
          _next = next;
          _logger = logger;
      }

      public async Task InvokeAsync(HttpContext context)
      {
          try
          {
              await _next(context);
          }
          catch (Exception ex)
          {
              await HandleExceptionAsync(context, ex);
          }
      }

      private async Task HandleExceptionAsync(HttpContext context, Exception exception)
      {
          _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

          var statusCode = exception switch
          {
              KeyNotFoundException => (int)HttpStatusCode.NotFound,
              UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
              ArgumentException => (int)HttpStatusCode.BadRequest,
              _ => (int)HttpStatusCode.InternalServerError
          };

          var response = new
          {
              error = exception.Message,
              statusCode = statusCode,
              // Only show stack trace in development
              details = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development"
                  ? exception.StackTrace
                  : null
          };

          context.Response.ContentType = "application/json";
          context.Response.StatusCode = statusCode;

          await context.Response.WriteAsync(JsonSerializer.Serialize(response));
      }
  }