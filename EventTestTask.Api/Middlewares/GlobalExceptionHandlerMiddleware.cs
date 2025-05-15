using System.Security.Authentication;
using System.Text.Json;

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
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(_.Message));
        }
        catch (Exception _) when (_ is KeyNotFoundException)
        {
            logger.LogInformation("Not found");
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(_.Message));
        }
        catch (Exception _) when (_ is AuthenticationException)
        {
            logger.LogInformation("Authentication failed");
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(_.Message));
        }
        catch (Exception _) when (_ is UnauthorizedAccessException)
        {
            logger.LogInformation("Unauthorized");
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(_.Message));
        }
        catch (Exception _) when (_ is NullReferenceException)
        {
            logger.LogInformation("NullReference");
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(_.Message));
        }
        catch (Exception _) when (_ is InvalidDataException)
        {
            logger.LogInformation("Invalid data");
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(_.Message));
        }
        catch (Exception _) when (_ is InvalidOperationException)
        {
            logger.LogInformation("Invalid operation");
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(_.Message));
        }
        catch (Exception _) when (_ is FluentValidation.ValidationException)
        {
            logger.LogInformation("FluentValidation exception");
            context.Response.ContentType = "application/json";
            
            await context.Response.WriteAsync(JsonSerializer.Serialize(_.Message));
        }
    }
}