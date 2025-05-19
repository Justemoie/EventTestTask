using System.Security.Authentication;
using System.Text.Json;
using BCrypt.Net;
using Microsoft.AspNetCore.Http;

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
            logger.LogInformation("Operation was canceled");
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(_.Message));
        }
        catch (Exception _) when (_ is KeyNotFoundException)
        {
            logger.LogInformation("Not found");
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(_.Message));
        }
        catch (Exception _) when (_ is AuthenticationException)
        {
            logger.LogInformation("Authentication failed");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(_.Message));
        }
        catch (Exception _) when (_ is UnauthorizedAccessException)
        {
            logger.LogInformation("Unauthorized");
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(_.Message));
        }
        catch (Exception _) when (_ is NullReferenceException)
        {
            logger.LogInformation("NullReference");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(_.Message));
        }
        catch (Exception _) when (_ is InvalidDataException)
        {
            logger.LogInformation("Invalid data");
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(_.Message));
        }
        catch (Exception _) when (_ is InvalidOperationException)
        {
            logger.LogInformation("Invalid operation");
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(_.Message));
        }
        catch (Exception _) when (_ is FluentValidation.ValidationException)
        {
            logger.LogInformation("FluentValidation exception");
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(_.Message));
        }
        catch (Exception _) when (_ is SaltParseException)
        {
            logger.LogInformation("Salt parse exception");
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