using System.Net.Http.Headers;
using AspireAppTemplate.Shared;
using Microsoft.Extensions.Options;

namespace AspireAppTemplate.ApiService.Services;

public class KeycloakConfiguration
{
    public string BaseUrl { get; set; } = string.Empty;
    public string Realm { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string AdminUsername { get; set; } = string.Empty;
    public string AdminPassword { get; set; } = string.Empty;
}

public class IdentityService
{
    private readonly HttpClient _httpClient;
    private readonly KeycloakConfiguration _config;
    private string? _cachedToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public IdentityService(HttpClient httpClient, IOptions<KeycloakConfiguration> options)
    {
        _httpClient = httpClient;
        _config = options.Value;
        
        if (string.IsNullOrEmpty(_config.BaseUrl)) throw new ArgumentNullException("Keycloak BaseUrl is not configured.");
        
        _httpClient.BaseAddress = new Uri(_config.BaseUrl.TrimEnd('/') + "/");
    }

    private async Task<string> GetAccessTokenAsync()
    {
        if (_cachedToken != null && DateTime.UtcNow < _tokenExpiry)
        {
            return _cachedToken;
        }

        var param = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("client_id", _config.ClientId),
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("username", _config.AdminUsername),
            new KeyValuePair<string, string>("password", _config.AdminPassword)
        };

        if (!string.IsNullOrEmpty(_config.ClientSecret))
        {
            param.Add(new KeyValuePair<string, string>("client_secret", _config.ClientSecret));
        }

        var content = new FormUrlEncodedContent(param);

        var response = await _httpClient.PostAsync($"realms/master/protocol/openid-connect/token", content);
        response.EnsureSuccessStatusCode();

        var tokenData = await response.Content.ReadFromJsonAsync<TokenResponse>();
        _cachedToken = tokenData!.access_token;
        _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenData.expires_in - 30); // Buffer of 30s

        return _cachedToken;
    }

    private async Task SetAuthHeaderAsync()
    {
        var token = await GetAccessTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    // --- Users ---

    public async Task<IEnumerable<KeycloakUser>> GetUsersAsync(string? search = null)
    {
        await SetAuthHeaderAsync();
        var url = $"admin/realms/{_config.Realm}/users";
        if (!string.IsNullOrEmpty(search)) url += $"?search={search}";
        
        var users = await _httpClient.GetFromJsonAsync<IEnumerable<KeycloakUser>>(url);
        return users ?? Enumerable.Empty<KeycloakUser>();
    }

    public async Task<bool> CreateUserAsync(KeycloakUser user)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync($"admin/realms/{_config.Realm}/users", user);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[IdentityService] Failed to create user: {response.StatusCode} - {error}");
        }
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteUserAsync(string id)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.DeleteAsync($"admin/realms/{_config.Realm}/users/{id}");
        if (!response.IsSuccessStatusCode)
        {
             var error = await response.Content.ReadAsStringAsync();
             Console.WriteLine($"[IdentityService] Failed to delete user: {response.StatusCode} - {error}");
        }
        return response.IsSuccessStatusCode;
    }

    // --- Roles ---

    public async Task<IEnumerable<KeycloakRole>> GetRolesAsync()
    {
        await SetAuthHeaderAsync();
        var roles = await _httpClient.GetFromJsonAsync<IEnumerable<KeycloakRole>>($"admin/realms/{_config.Realm}/roles");
        return roles ?? Enumerable.Empty<KeycloakRole>();
    }

    public async Task<bool> CreateRoleAsync(KeycloakRole role)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync($"admin/realms/{_config.Realm}/roles", role);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[IdentityService] Failed to create role: {response.StatusCode} - {error}");
        }
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteRoleAsync(string name)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.DeleteAsync($"admin/realms/{_config.Realm}/roles/{name}");
        return response.IsSuccessStatusCode;
    }

    // --- Mappings ---

    public async Task<bool> AssignRoleToUserAsync(string userId, string roleName)
    {
        await SetAuthHeaderAsync();
        
        // 1. Get role details to get its ID
        var role = await _httpClient.GetFromJsonAsync<KeycloakRole>($"admin/realms/{_config.Realm}/roles/{roleName}");
        if (role == null) return false;

        // 2. Assign to user (POST to role-mappings)
        var response = await _httpClient.PostAsJsonAsync($"admin/realms/{_config.Realm}/users/{userId}/role-mappings/realm", new[] { role });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RemoveRoleFromUserAsync(string userId, string roleName)
    {
        await SetAuthHeaderAsync();
        
        var role = await _httpClient.GetFromJsonAsync<KeycloakRole>($"admin/realms/{_config.Realm}/roles/{roleName}");
        if (role == null) return false;

        var request = new HttpRequestMessage(HttpMethod.Delete, $"admin/realms/{_config.Realm}/users/{userId}/role-mappings/realm")
        {
            Content = JsonContent.Create(new[] { role })
        };
        
        var response = await _httpClient.SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    public async Task<IEnumerable<KeycloakRole>> GetUserRolesAsync(string userId)
    {
        await SetAuthHeaderAsync();
        var roles = await _httpClient.GetFromJsonAsync<IEnumerable<KeycloakRole>>($"admin/realms/{_config.Realm}/users/{userId}/role-mappings/realm");
        return roles ?? Enumerable.Empty<KeycloakRole>();
    }

    public async Task<bool> UpdateUserAsync(string id, KeycloakUser user)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"admin/realms/{_config.Realm}/users/{id}", user);
        return response.IsSuccessStatusCode;
    }
}
