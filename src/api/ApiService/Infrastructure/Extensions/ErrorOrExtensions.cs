using ErrorOr;
using FastEndpoints;

namespace AspireAppTemplate.ApiService.Infrastructure.Extensions;

public static class ErrorOrExtensions
{
    public static async Task SendResultAsync<T>(
        this IEndpoint ep,
        ErrorOr<T> result,
        Func<T, CancellationToken, Task>? onSuccess = null,
        CancellationToken ct = default)
    {
        if (!result.IsError)
        {
            if (onSuccess is not null)
            {
                await onSuccess(result.Value, ct);
            }
            else
            {
                await ep.HttpContext.Response.SendAsync(result.Value, 200, cancellation: ct);
            }
            return;
        }

        var firstError = result.FirstError;

        var statusCode = firstError.Type switch
        {
            ErrorType.NotFound => 404,
            ErrorType.Validation => 400,
            ErrorType.Conflict => 409,
            ErrorType.Unauthorized => 401,
            ErrorType.Forbidden => 403,
            _ => 500
        };

        if (statusCode == 400 && result.ErrorsOrEmptyList.Count > 0)
        {
            foreach (var error in result.ErrorsOrEmptyList)
            {
                ep.ValidationFailures.Add(new(error.Code, error.Description));
            }
            await ep.HttpContext.Response.SendErrorsAsync(ep.ValidationFailures, cancellation: ct);
        }
        else
        {
            // Create a standard ProblemDetails for non-validation errors
            var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Status = statusCode,
                Title = firstError.Code,
                Detail = firstError.Description,
                Instance = ep.HttpContext.Request.Path
            };
            
            await ep.HttpContext.Response.SendAsync(problemDetails, statusCode, cancellation: ct);
        }
    }
}
