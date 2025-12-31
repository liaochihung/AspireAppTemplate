using Microsoft.AspNetCore.Components;
using System.Net;

namespace AspireAppTemplate.Web.Components.Pages;

/// <summary>
/// Base class for pages that call authenticated APIs.
/// Provides helper methods to handle 401 Unauthorized responses.
/// </summary>
public abstract class AuthenticatedComponentBase : ComponentBase
{
    [Inject] protected NavigationManager Nav { get; set; } = default!;
    [Inject] protected ILogger<AuthenticatedComponentBase> Logger { get; set; } = default!;

    /// <summary>
    /// Execute an async action and handle 401 Unauthorized by redirecting to logout.
    /// </summary>
    protected async Task ExecuteWithAuthHandlingAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            Logger.LogWarning("401 Unauthorized detected, redirecting to logout");
            Nav.NavigateTo("authentication/logout", forceLoad: true);
        }
    }

    /// <summary>
    /// Execute an async function and handle 401 Unauthorized by redirecting to logout.
    /// Returns default(T) if 401 is encountered.
    /// </summary>
    protected async Task<T?> ExecuteWithAuthHandlingAsync<T>(Func<Task<T>> func)
    {
        try
        {
            return await func();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            Logger.LogWarning("401 Unauthorized detected, redirecting to logout");
            Nav.NavigateTo("authentication/logout", forceLoad: true);
            return default;
        }
    }
}
