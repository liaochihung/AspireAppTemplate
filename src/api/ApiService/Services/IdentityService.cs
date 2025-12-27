using System.Net.Http.Headers;
using AspireAppTemplate.Shared;
using ErrorOr;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

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

    /// <summary>
    /// Creates an instance of IdentityService
    /// </summary>
    /// <param name="httpClient">HTTP client for making requests</param>
    /// <param name="options">Keycloak configuration options</param>
    /// <exception cref="InvalidOperationException">Thrown when Keycloak BaseUrl is not configured</exception>
    public IdentityService(HttpClient httpClient, IOptions<KeycloakConfiguration> options)
    {
        _httpClient = httpClient;
        _config = options.Value;
        
        if (string.IsNullOrEmpty(_config.BaseUrl)) 
            throw new InvalidOperationException("Keycloak BaseUrl is not configured.");
        
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

    public async Task<ErrorOr<IEnumerable<KeycloakUser>>> GetUsersAsync(string? search = null)
    {
        await SetAuthHeaderAsync();
        var url = $"admin/realms/{_config.Realm}/users";
        if (!string.IsNullOrEmpty(search)) url += $"?search={search}";
        
        var users = await _httpClient.GetFromJsonAsync<IEnumerable<KeycloakUser>>(url);
        return users?.ToList() ?? new List<KeycloakUser>();
    }

    public async Task<ErrorOr<Created>> CreateUserAsync(KeycloakUser user)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync($"admin/realms/{_config.Realm}/users", user);
        
        if (response.IsSuccessStatusCode)
        {
            return Result.Created;
        }

        var error = await response.Content.ReadAsStringAsync();
        if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            return Error.Conflict(description: error);
        }
        
        return Error.Failure(description: $"Failed to create user: {response.StatusCode} - {error}");
    }

    public async Task<ErrorOr<Deleted>> DeleteUserAsync(string id)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.DeleteAsync($"admin/realms/{_config.Realm}/users/{id}");
        
        if (response.IsSuccessStatusCode)
        {
            return Result.Deleted;
        }

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Error.NotFound(description: "User not found.");
        }

        var error = await response.Content.ReadAsStringAsync();
        return Error.Failure(description: $"Failed to delete user: {response.StatusCode} - {error}");
    }

    // --- Roles ---

    public async Task<ErrorOr<IEnumerable<KeycloakRole>>> GetRolesAsync()
    {
        try
        {
            await SetAuthHeaderAsync();
            var response = await _httpClient.GetAsync($"admin/realms/{_config.Realm}/roles");
            
            if (!response.IsSuccessStatusCode)
            {
               if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                   return Error.NotFound(description: "Roles or Realm not found.");
               if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                   return Error.Unauthorized(description: "Keycloak unauthorized.");

               return Error.Failure(description: $"Failed to get roles: {response.StatusCode}");
            }

            var roles = await response.Content.ReadFromJsonAsync<IEnumerable<KeycloakRole>>();
            return roles?.ToList() ?? new List<KeycloakRole>();
        }
        catch (Exception ex)
        {
            return Error.Unexpected(description: ex.Message);
        }
    }

    public async Task<ErrorOr<Created>> CreateRoleAsync(KeycloakRole role)
    {
        try 
        {
            await SetAuthHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync($"admin/realms/{_config.Realm}/roles", role);
            
            if (response.IsSuccessStatusCode)
            {
                return Result.Created;
            }

            var error = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                 return Error.Conflict(description: "Role already exists or conflict occurred.");
            }

            return Error.Failure(description: $"Failed to create role: {response.StatusCode} - {error}");
        }
        catch (Exception ex)
        {
            return Error.Unexpected(description: ex.Message);
        }
    }

    public async Task<ErrorOr<Deleted>> DeleteRoleAsync(string name)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.DeleteAsync($"admin/realms/{_config.Realm}/roles/{name}");
        
        if (response.IsSuccessStatusCode)
        {
            return Result.Deleted;
        }
        
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Error.NotFound(description: "Role not found.");
        }

        return Error.Failure(description: $"Failed to delete role: {response.StatusCode}");
    }

    // --- Mappings ---

    public async Task<ErrorOr<Success>> AssignRoleToUserAsync(string userId, string roleName)
    {
        await SetAuthHeaderAsync();
        
        // 1. Get role details to get its ID
        // Note: Ideally we should handle if this first call fails too.
        // For simplicity, if GetFromJsonAsync fails (e.g. 404), it throws extension exception usually or returns null? 
        // HttpClient.GetFromJsonAsync throws HttpRequestException on non-success status codes if not handled, 
        // but here we might want to be safer. However, preserving structure:
        
        // Let's wrap this in try-catch or ensure check
        KeycloakRole? role = null;
        try 
        {
             role = await _httpClient.GetFromJsonAsync<KeycloakRole>($"admin/realms/{_config.Realm}/roles/{roleName}");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
             return Error.NotFound(description: $"Role '{roleName}' not found.");
        }

        if (role == null) return Error.NotFound(description: $"Role '{roleName}' not found.");

        // 2. Assign to user (POST to role-mappings)
        var response = await _httpClient.PostAsJsonAsync($"admin/realms/{_config.Realm}/users/{userId}/role-mappings/realm", new[] { role });
        
        if (response.IsSuccessStatusCode)
        {
            return Result.Success;
        }

        var error = await response.Content.ReadAsStringAsync();
        return Error.Failure(description: $"Failed to assign role: {response.StatusCode} - {error}");
    }

    public async Task<ErrorOr<Success>> RemoveRoleFromUserAsync(string userId, string roleName)
    {
        await SetAuthHeaderAsync();
        
        KeycloakRole? role = null;
        try 
        {
             role = await _httpClient.GetFromJsonAsync<KeycloakRole>($"admin/realms/{_config.Realm}/roles/{roleName}");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
             return Error.NotFound(description: $"Role '{roleName}' not found.");
        }

        if (role == null) return Error.NotFound(description: $"Role '{roleName}' not found.");

        var request = new HttpRequestMessage(HttpMethod.Delete, $"admin/realms/{_config.Realm}/users/{userId}/role-mappings/realm")
        {
            Content = JsonContent.Create(new[] { role })
        };
        
        var response = await _httpClient.SendAsync(request);
        
        if (response.IsSuccessStatusCode)
        {
            return Result.Success;
        }

        return Error.Failure(description: $"Failed to remove role: {response.StatusCode}");
    }

    public async Task<ErrorOr<IEnumerable<KeycloakRole>>> GetUserRolesAsync(string userId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync($"admin/realms/{_config.Realm}/users/{userId}/role-mappings/realm");
        
        if (!response.IsSuccessStatusCode)
        {
             if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
             {
                 return Error.NotFound(description: "User not found.");
             }
             return Error.Failure(description: $"Failed to get user roles: {response.StatusCode}");
        }

        var roles = await response.Content.ReadFromJsonAsync<IEnumerable<KeycloakRole>>();
        return roles?.ToList() ?? new List<KeycloakRole>();
    }

    public async Task<ErrorOr<Success>> UpdateUserAsync(string id, KeycloakUser user)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"admin/realms/{_config.Realm}/users/{id}", user);

        if (response.IsSuccessStatusCode)
        {
            return Result.Success;
        }
        
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
             return Error.NotFound(description: "User not found.");
        }
        
        var error = await response.Content.ReadAsStringAsync();
        return Error.Failure(description: $"Failed to update user: {response.StatusCode} - {error}");
    }
}
