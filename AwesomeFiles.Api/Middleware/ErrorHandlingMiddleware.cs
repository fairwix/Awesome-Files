using System.Text.Json;
using AwesomeFiles.Application.Exceptions;
using AwesomeFiles.Domain.Exceptions;
using ApplicationException = AwesomeFiles.Application.Exceptions.ApplicationException;

namespace AwesomeFiles.Api.Middleware;
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
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
        catch (ApplicationException ex)
        {
            _logger.LogWarning(ex, "Application error: {Message}", ex.Message);
            await HandleExceptionAsync(context, StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain error: {Message}", ex.Message);
            await HandleExceptionAsync(context, StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogWarning(ex, "File not found: {Message}", ex.Message);
            await HandleExceptionAsync(context, StatusCodes.Status404NotFound, ex.Message);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogInformation("Operation cancelled: {Message}", ex.Message);
            await HandleExceptionAsync(context, StatusCodes.Status499ClientClosedRequest, "Request cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            await HandleExceptionAsync(context, StatusCodes.Status500InternalServerError, "Internal server error");
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        
        var problemDetails = new
        {
            type = $"https://httpstatuses.com/{statusCode}",
            title = GetTitleForStatusCode(statusCode),
            status = statusCode,
            detail = message,
            instance = context.Request.Path
        };
        
        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
    }

    private static string GetTitleForStatusCode(int statusCode) => statusCode switch
    {
        400 => "Bad Request",
        404 => "Not Found",
        499 => "Client Closed Request",
        500 => "Internal Server Error",
        _ => "Error"
    };
}