using System.Net;
using System.Text.Json;

namespace VehicleRentalSystem.Middleware;

/// <summary>
/// Global exception handling middleware.
/// Catches unhandled exceptions, logs them via Serilog,
/// and returns a user-friendly JSON error or redirects to the error page.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public GlobalExceptionMiddleware(RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IWebHostEnvironment env)
    {
        _next   = next;
        _logger = logger;
        _env    = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unhandled exception on {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        // API / AJAX requests → JSON error
        if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
            context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.ContentType = "application/json";
            var payload = new
            {
                success = false,
                message = _env.IsDevelopment()
                    ? exception.Message
                    : "An internal error occurred. Please contact support."
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
        else
        {
            // MVC requests → redirect to error page
            context.Response.Redirect("/Home/Error");
        }
    }
}

/// <summary>Extension method to register the middleware cleanly.</summary>
public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
        => app.UseMiddleware<GlobalExceptionMiddleware>();
}
