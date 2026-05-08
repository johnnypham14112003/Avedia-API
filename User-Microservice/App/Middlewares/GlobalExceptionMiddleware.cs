using BusinessLogic.Extensions.Exceptions;
using BusinessLogic.Models.Generic;

namespace App.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);

            // Handle 401/403 from framework (eg: [Authorize])
            if (IsAuthError(context.Response.StatusCode) && !context.Response.HasStarted)
            {
                await HandleAuthErrorAsync(context);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Something went wrong while processing {RequestPath}", context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    // Pattern Matching (is ... or ...)
    private static bool IsAuthError(int statusCode) =>
        statusCode is StatusCodes.Status401Unauthorized or StatusCodes.Status403Forbidden;

    private async Task HandleAuthErrorAsync(HttpContext context)
    {
        var message = context.Response.StatusCode switch
        {
            StatusCodes.Status401Unauthorized => "Please check again your authorize token (Unauthorized).",
            StatusCodes.Status403Forbidden => "You don't have permission to do this action (Forbidden).",
            _ => "You don't have permission"
        };

        await WriteResponseAsync(context, context.Response.StatusCode, message);
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        if (context.Response.HasStarted)
        {
            _logger.LogWarning("The response has already started, the error middleware will not be executed.");
            return;
        }

        var (statusCode, message) = ex switch
        {
            BadRequestException badReqEx => (StatusCodes.Status400BadRequest, badReqEx.Message),
            NotFoundException notFoundEx => (StatusCodes.Status404NotFound, notFoundEx.Message),
            UnauthorizedException unAuthEx => (StatusCodes.Status401Unauthorized, unAuthEx.Message),
            ConflictException conflictEx => (StatusCodes.Status409Conflict, conflictEx.Message),

            // Default error 500.
            //_ => (StatusCodes.Status500InternalServerError, ex.Message) //For develop environment only for secure
            _ => (StatusCodes.Status500InternalServerError, "Server Error catch from Middleware")
        };

        await WriteResponseAsync(context, statusCode, message);
    }

    private static async Task WriteResponseAsync(HttpContext context, int statusCode, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var responseModel = new ApiResult<object>
        {
            StatusCode = statusCode,
            Message = message,
            Data = null
        };

        // Use WriteAsJsonAsync (System.Text.Json)(HttpContext .Net 10) to write object directly to Stream
        // Don't use serialize to string (Newtonsoft) <= This cost more RAM
        await context.Response.WriteAsJsonAsync(responseModel);
    }
}

