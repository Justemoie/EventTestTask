using System.Security.Authentication;
using System.Text.Json;
using BCrypt.Net;
using FluentValidation;

namespace EventTestTask.Api.Middlewares;

public class GlobalExceptionHandlerMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionHandlerMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception _) when (_ is OperationCanceledException or TaskCanceledException)
        {
            logger.LogError("Operation was canceled");
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(_.Message));
        }
        catch (Exception _) when (_ is KeyNotFoundException)
        {
            logger.LogError("Not found");
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(_.Message));
        }
        catch (Exception _) when (_ is AuthenticationException)
        {
            logger.LogError("Authentication failed");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(_.Message));
        }
        catch (Exception _) when (_ is UnauthorizedAccessException)
        {
            logger.LogError("Unauthorized");
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(_.Message));
        }
        catch (Exception _) when (_ is NullReferenceException)
        {
            logger.LogError("NullReference");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(_.Message));
        }
        catch (Exception _) when (_ is InvalidDataException)
        {
            logger.LogError("Invalid data");
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(_.Message));
        }
        catch (Exception _) when (_ is InvalidOperationException)
        {
            logger.LogError("Invalid operation");
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(_.Message));
        }
        catch (Exception _) when (_ is ValidationException)
        {
            logger.LogError("FluentValidation exception");
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(_.Message));
        }
        catch (Exception _) when (_ is SaltParseException)
        {
            logger.LogError("Salt parse exception");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize("Invalid password or email"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize("An unexpected error occurred."));
        }
    }
}