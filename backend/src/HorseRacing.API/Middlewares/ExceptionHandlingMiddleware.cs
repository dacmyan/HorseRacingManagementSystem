using System.Net;
using System.Text.Json;

namespace HorseRacing.API.Middlewares;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, message) = ex switch
        {
            KeyNotFoundException     => (HttpStatusCode.NotFound,            ex.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized,     ex.Message),
            ArgumentException        => (HttpStatusCode.BadRequest,          ex.Message),
            InvalidOperationException => (HttpStatusCode.UnprocessableEntity, ex.Message),
            _                        => (HttpStatusCode.InternalServerError, "An internal system error occurred.")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode  = (int)statusCode;

        var body = JsonSerializer.Serialize(new
        {
            statusCode = (int)statusCode,
            message
        });

        return context.Response.WriteAsync(body);
    }
}
