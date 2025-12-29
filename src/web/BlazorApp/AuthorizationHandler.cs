using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Globalization;
using System.Net.Http.Headers;
using AspireAppTemplate.Web.Infrastructure.Authentication;

namespace AspireAppTemplate.Web;

/// <summary>
/// HTTP Message Handler that attaches access tokens to outgoing requests.
/// Automatically refreshes tokens when they are about to expire.
/// </summary>
public class AuthorizationHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuthorizationHandler> _logger;
    private readonly TokenCacheService _tokenCache;

    private static readonly TimeSpan TokenRefreshBuffer = TimeSpan.FromMinutes(1);

    public AuthorizationHandler(
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuthorizationHandler> logger,
        TokenCacheService tokenCache)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _tokenCache = tokenCache;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext is null)
        {
            _logger.LogWarning("HttpContext is null - cannot attach token");
            return await base.SendAsync(request, cancellationToken);
        }

        try
        {
            var accessToken = await GetValidAccessTokenAsync(httpContext, cancellationToken);

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                _logger.LogWarning("No valid access_token available for request to {Uri}", request.RequestUri);
            }
            else
            {
                _logger.LogDebug("Attaching Bearer token to request: {Uri}", request.RequestUri);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting access token for request to {Uri}", request.RequestUri);
        }

        return await base.SendAsync(request, cancellationToken);
    }

    private async Task<string?> GetValidAccessTokenAsync(HttpContext httpContext, CancellationToken cancellationToken)
    {
        var sessionId = GetSessionId(httpContext);

        // 1. Check if we have a cached refreshed token
        var cachedTokens = _tokenCache.GetTokens(sessionId);
        if (cachedTokens != null && cachedTokens.ExpiresAt > DateTimeOffset.UtcNow.Add(TokenRefreshBuffer))
        {
            _logger.LogDebug("Using cached access token, expires at {ExpiresAt}", cachedTokens.ExpiresAt);
            return cachedTokens.AccessToken;
        }

        // 2. Get token from authentication properties (cookie)
        var accessToken = await httpContext.GetTokenAsync("access_token");
        var expiresAtStr = await httpContext.GetTokenAsync("expires_at");

        if (string.IsNullOrEmpty(accessToken))
        {
            return null;
        }

        // 3. Check if token needs refresh
        if (!string.IsNullOrEmpty(expiresAtStr) &&
            DateTimeOffset.TryParse(expiresAtStr, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var expiresAt) &&
            expiresAt - DateTimeOffset.UtcNow < TokenRefreshBuffer)
        {
            _logger.LogInformation("Access token expires at {ExpiresAt}, refreshing proactively", expiresAt);
            var refreshedToken = await RefreshTokenAsync(httpContext, sessionId, cancellationToken);
            if (refreshedToken != null)
            {
                return refreshedToken;
            }
            // Fall through to return current token if refresh fails
        }

        return accessToken;
    }

    private async Task<string?> RefreshTokenAsync(HttpContext httpContext, string sessionId, CancellationToken cancellationToken)
    {
        var refreshToken = await httpContext.GetTokenAsync("refresh_token");
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            _logger.LogWarning("No refresh_token available for token refresh");
            return null;
        }

        try
        {
            // Get OIDC configuration
            var oidcOptions = httpContext.RequestServices
                .GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<OpenIdConnectOptions>>()
                .Get(OpenIdConnectDefaults.AuthenticationScheme);

            var configuration = await oidcOptions.ConfigurationManager!.GetConfigurationAsync(cancellationToken);
            var tokenEndpoint = configuration.TokenEndpoint;

            if (string.IsNullOrEmpty(tokenEndpoint))
            {
                _logger.LogError("Token endpoint not found in OIDC configuration");
                return null;
            }

            // Build refresh token request
            using var httpClient = new HttpClient();
            var tokenRequest = new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["client_id"] = oidcOptions.ClientId!,
                ["refresh_token"] = refreshToken
            };

            if (!string.IsNullOrEmpty(oidcOptions.ClientSecret))
            {
                tokenRequest["client_secret"] = oidcOptions.ClientSecret;
            }

            var response = await httpClient.PostAsync(
                tokenEndpoint,
                new FormUrlEncodedContent(tokenRequest),
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Token refresh failed: {StatusCode} - {Error}",
                    response.StatusCode, errorContent);
                return null;
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: cancellationToken);
            if (tokenResponse?.AccessToken == null)
            {
                _logger.LogWarning("Token refresh response missing access_token");
                return null;
            }

            // Calculate expiration
            var newExpiresAt = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn);

            // Store in cache
            var cachedTokens = new CachedTokens
            {
                AccessToken = tokenResponse.AccessToken,
                RefreshToken = tokenResponse.RefreshToken,
                ExpiresAt = newExpiresAt
            };
            _tokenCache.StoreTokens(sessionId, cachedTokens);

            _logger.LogInformation("Successfully refreshed access token, new expiry in {ExpiresIn}s (cached for persistence)",
                tokenResponse.ExpiresIn);

            return tokenResponse.AccessToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing access token");
            return null;
        }
    }

    private static string GetSessionId(HttpContext httpContext)
    {
        // Use the session cookie or user identity as a key
        // This ensures multiple users don't share cached tokens
        var user = httpContext.User;
        if (user.Identity?.IsAuthenticated == true)
        {
            var sub = user.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(sub))
            {
                return sub;
            }
        }

        // Fallback to session id from cookie
        return httpContext.Session?.Id ?? "anonymous";
    }

    private sealed class TokenResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}