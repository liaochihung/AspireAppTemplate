using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;

namespace AspireAppTemplate.Web;

/// <summary>
/// HTTP Message Handler that attaches access tokens to outgoing requests
/// and automatically refreshes expired tokens using the refresh_token grant.
/// </summary>
public class AuthorizationHandler(
    IHttpContextAccessor httpContextAccessor,
    ILogger<AuthorizationHandler> logger) : DelegatingHandler
{
    // Refresh token when it expires within this buffer time
    private static readonly TimeSpan TokenRefreshBuffer = TimeSpan.FromMinutes(1);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext is null)
        {
            logger.LogWarning("HttpContext is null - cannot attach token");
            return await base.SendAsync(request, cancellationToken);
        }

        try
        {
            var accessToken = await GetOrRefreshAccessTokenAsync(httpContext, cancellationToken);

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                logger.LogWarning("No access_token available for request to {Uri}", request.RequestUri);
            }
            else
            {
                logger.LogInformation("Attaching Bearer token to request: {Uri}", request.RequestUri);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting access token from HttpContext");
        }

        var response = await base.SendAsync(request, cancellationToken);

        // If we get a 401, try to refresh token and retry once
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            logger.LogWarning("Received 401 from {Uri}, attempting token refresh and retry", request.RequestUri);

            try
            {
                var newAccessToken = await ForceRefreshTokenAsync(httpContext, cancellationToken);

                if (!string.IsNullOrWhiteSpace(newAccessToken))
                {
                    // Clone the request (can't reuse the original) and retry
                    var retryRequest = await CloneHttpRequestMessageAsync(request);
                    retryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newAccessToken);

                    logger.LogInformation("Retrying request to {Uri} with refreshed token", request.RequestUri);
                    response = await base.SendAsync(retryRequest, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to refresh token after 401 response");
            }
        }

        return response;
    }

    private async Task<string?> GetOrRefreshAccessTokenAsync(HttpContext httpContext, CancellationToken cancellationToken)
    {
        var expiresAtStr = await httpContext.GetTokenAsync("expires_at");

        if (!string.IsNullOrEmpty(expiresAtStr) &&
            DateTimeOffset.TryParse(expiresAtStr, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var expiresAt) &&
            expiresAt - DateTimeOffset.UtcNow < TokenRefreshBuffer)
        {
            // Token is about to expire, refresh proactively
            logger.LogInformation("Access token expires at {ExpiresAt}, refreshing proactively", expiresAt);
            return await ForceRefreshTokenAsync(httpContext, cancellationToken);
        }

        return await httpContext.GetTokenAsync("access_token");
    }

    private async Task<string?> ForceRefreshTokenAsync(HttpContext httpContext, CancellationToken cancellationToken)
    {
        var refreshToken = await httpContext.GetTokenAsync("refresh_token");

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            logger.LogWarning("No refresh_token available for token refresh");
            return null;
        }

        // Get OIDC configuration to find token endpoint
        var oidcOptions = httpContext.RequestServices
            .GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<OpenIdConnectOptions>>()
            .Get(OpenIdConnectDefaults.AuthenticationScheme);

        var configuration = await oidcOptions.ConfigurationManager!.GetConfigurationAsync(cancellationToken);
        var tokenEndpoint = configuration.TokenEndpoint;

        if (string.IsNullOrEmpty(tokenEndpoint))
        {
            logger.LogError("Token endpoint not found in OIDC configuration");
            return null;
        }

        logger.LogDebug("Refreshing token using endpoint: {TokenEndpoint}", tokenEndpoint);

        // Build the refresh token request
        using var httpClient = new HttpClient();
        var tokenRequest = new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["client_id"] = oidcOptions.ClientId!,
            ["refresh_token"] = refreshToken
        };

        // Add client secret if configured
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
            logger.LogWarning("Token refresh failed with status {StatusCode}: {Error}",
                response.StatusCode, errorContent);
            return null;
        }

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: cancellationToken);

        if (tokenResponse is null || string.IsNullOrEmpty(tokenResponse.AccessToken))
        {
            logger.LogWarning("Token refresh response missing access_token");
            return null;
        }

        // Update the stored tokens in the authentication properties
        var authenticateResult = await httpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        if (authenticateResult.Succeeded && authenticateResult.Properties != null)
        {
            var properties = authenticateResult.Properties;

            properties.UpdateTokenValue("access_token", tokenResponse.AccessToken);

            if (!string.IsNullOrEmpty(tokenResponse.RefreshToken))
            {
                properties.UpdateTokenValue("refresh_token", tokenResponse.RefreshToken);
            }

            if (tokenResponse.ExpiresIn > 0)
            {
                var newExpiresAt = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
                properties.UpdateTokenValue("expires_at", newExpiresAt.ToString("o", CultureInfo.InvariantCulture));
            }

            // Re-sign in to persist the updated tokens
            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                authenticateResult.Principal!,
                properties);

            logger.LogInformation("Successfully refreshed access token, new expiry in {ExpiresIn}s", tokenResponse.ExpiresIn);
        }

        return tokenResponse.AccessToken;
    }

    private static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);

        // Copy headers
        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        // Copy content if any
        if (request.Content != null)
        {
            var contentBytes = await request.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(contentBytes);

            foreach (var header in request.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return clone;
    }

    private sealed class TokenResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("token_type")]
        public string? TokenType { get; set; }
    }
}