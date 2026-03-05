using System.Net;
using System.Text.Json;

namespace WebApplication1.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // This tells .NET to continue running the rest of the app (like controllers)
                await _next(context);
            }
            catch (Exception ex)
            {
                // If ANYTHING fails, it falls back up to here!

                // 1. Log the error to console/terminal
                _logger.LogError(ex, "An unhandled exception has occurred: {Message}", ex.Message);

                // 2. Format the response for the frontend
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var errorResponse = new
            {
                message = "An unexpected error occurred while processing your request.",
                details = exception.Message
            };

            var jsonResponse = JsonSerializer.Serialize(errorResponse);
            return context.Response.WriteAsync(jsonResponse);
        }
    }
}
