using Microsoft.Extensions.Caching.Memory;
using System.Globalization;

namespace AspireAppTemplate.Web.Infrastructure.Authentication;

/// <summary>
/// Provides a lightweight cache for refreshed access tokens.
/// This allows tokens to be shared across requests and persisted to cookies when possible.
/// </summary>
public class TokenCacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<TokenCacheService> _logger;

    // Cache keys - keyed by session id for multi-user support
    private const string TokenCacheKeyPrefix = "RefreshedToken_";

    public TokenCacheService(IMemoryCache cache, ILogger<TokenCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Stores refreshed tokens in the cache.
    /// </summary>
    public void StoreTokens(string sessionId, CachedTokens tokens)
    {
        var cacheKey = TokenCacheKeyPrefix + sessionId;

        var cacheOptions = new MemoryCacheEntryOptions
        {
            // Keep in cache until token expires, plus a small buffer
            AbsoluteExpiration = tokens.ExpiresAt.AddMinutes(1)
        };

        _cache.Set(cacheKey, tokens, cacheOptions);
        _logger.LogDebug("Stored refreshed tokens in cache for session {SessionId}, expires at {ExpiresAt}",
            sessionId, tokens.ExpiresAt);
    }

    /// <summary>
    /// Attempts to retrieve cached tokens.
    /// </summary>
    public CachedTokens? GetTokens(string sessionId)
    {
        var cacheKey = TokenCacheKeyPrefix + sessionId;
        return _cache.Get<CachedTokens>(cacheKey);
    }

    /// <summary>
    /// Removes tokens from cache (e.g., after persisting to cookie).
    /// </summary>
    public void RemoveTokens(string sessionId)
    {
        var cacheKey = TokenCacheKeyPrefix + sessionId;
        _cache.Remove(cacheKey);
        _logger.LogDebug("Removed tokens from cache for session {SessionId}", sessionId);
    }

    /// <summary>
    /// Checks if there are pending tokens to be persisted.
    /// </summary>
    public bool HasPendingTokens(string sessionId)
    {
        var cacheKey = TokenCacheKeyPrefix + sessionId;
        return _cache.TryGetValue(cacheKey, out _);
    }
}

/// <summary>
/// Represents cached token data after a refresh operation.
/// </summary>
public sealed record CachedTokens
{
    public required string AccessToken { get; init; }
    public string? RefreshToken { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }

    public string ExpiresAtString => ExpiresAt.ToString("o", CultureInfo.InvariantCulture);
}
