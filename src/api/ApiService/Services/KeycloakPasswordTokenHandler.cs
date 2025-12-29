using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace AspireAppTemplate.ApiService.Services;

public class KeycloakPasswordTokenHandler(
    IOptions<KeycloakAdminConfiguration> options,
    ILogger<KeycloakPasswordTokenHandler> logger) : DelegatingHandler
{
    private readonly KeycloakAdminConfiguration _config = options.Value;
    private string? _cachedToken;
    private DateTime _tokenExpiry = DateTime.MinValue;
    private static readonly HttpClient _tokenClient = new();

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try 
        {
            var token = await GetAccessTokenAsync(cancellationToken);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        catch (Exception ex)
        {
             logger.LogError(ex, "Failed to attach access token to request.");
             throw new InvalidOperationException("Unable to obtain Keycloak admin token for API request.", ex); 
        }
        return await base.SendAsync(request, cancellationToken);
    }

    private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        if (_cachedToken != null && DateTime.UtcNow < _tokenExpiry)
        {
            return _cachedToken;
        }

        var baseUrl = _config.AuthServerUrl.TrimEnd('/');
        var tokenUrl = $"{baseUrl}/realms/{_config.Realm}/protocol/openid-connect/token";
        
        logger.LogInformation("Fetching Keycloak Admin Token from {TokenUrl} for user {AdminUser}", tokenUrl, _config.AdminUsername);

        var param = new List<KeyValuePair<string, string>>
        {
            new("client_id", _config.Resource),
            new("grant_type", "password"),
            new("username", _config.AdminUsername),
            new("password", _config.AdminPassword)
        };

        using var content = new FormUrlEncodedContent(param);
        
        var response = await _tokenClient.PostAsync(tokenUrl, content, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
             var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
             logger.LogError("Failed to get token. Status: {StatusCode}, Body: {Body}", response.StatusCode, errorBody);
             response.EnsureSuccessStatusCode(); 
        }

        var tokenData = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: cancellationToken);
        if (tokenData == null) throw new InvalidOperationException("Failed to deserialize token response.");

        _cachedToken = tokenData.access_token;
        _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenData.expires_in - 30);
        
        logger.LogInformation("Successfully acquired Keycloak Admin Token. Expires in {ExpiresIn}s", tokenData.expires_in);

        return _cachedToken;
    }

    private sealed record TokenResponse(string access_token, int expires_in);
}
