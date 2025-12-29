using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using AspireAppTemplate.Web.Infrastructure.Authentication;

namespace AspireAppTemplate.Web.Infrastructure.Middleware;

/// <summary>
/// Middleware that persists cached tokens to cookies when appropriate.
/// This runs early in the pipeline when we can still modify cookies.
/// </summary>
public class TokenPersistenceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TokenPersistenceMiddleware> _logger;

    public TokenPersistenceMiddleware(RequestDelegate next, ILogger<TokenPersistenceMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, TokenCacheService tokenCache)
    {
        // Only process for authenticated users
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var sessionId = GetSessionId(context);
            var cachedTokens = tokenCache.GetTokens(sessionId);

            if (cachedTokens != null)
            {
                // We have cached tokens that need to be persisted to cookies
                await PersistTokensToCookieAsync(context, cachedTokens, sessionId, tokenCache);
            }
        }

        await _next(context);
    }

    private async Task PersistTokensToCookieAsync(
        HttpContext context,
        CachedTokens cachedTokens,
        string sessionId,
        TokenCacheService tokenCache)
    {
        try
        {
            var authenticateResult = await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!authenticateResult.Succeeded || authenticateResult.Properties == null)
            {
                return;
            }

            var properties = authenticateResult.Properties;

            // Update token values
            properties.UpdateTokenValue("access_token", cachedTokens.AccessToken);

            if (!string.IsNullOrEmpty(cachedTokens.RefreshToken))
            {
                properties.UpdateTokenValue("refresh_token", cachedTokens.RefreshToken);
            }

            properties.UpdateTokenValue("expires_at", cachedTokens.ExpiresAtString);

            // Persist to cookie
            await context.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                authenticateResult.Principal!,
                properties);

            // Remove from cache after successful persistence
            tokenCache.RemoveTokens(sessionId);

            _logger.LogInformation("Persisted refreshed tokens to cookie, expires at {ExpiresAt}", cachedTokens.ExpiresAt);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to persist tokens to cookie (will retry on next request)");
            // Don't remove from cache - we'll try again next request
        }
    }

    private static string GetSessionId(HttpContext context)
    {
        var user = context.User;
        if (user.Identity?.IsAuthenticated == true)
        {
            var sub = user.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(sub))
            {
                return sub;
            }
        }
        return context.Session?.Id ?? "anonymous";
    }
}
