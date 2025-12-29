using AspireAppTemplate.Shared;
using ErrorOr;
using Keycloak.AuthServices.Sdk.Admin.Models;
using Microsoft.Extensions.Options;

namespace AspireAppTemplate.ApiService.Services;

public class KeycloakAdminConfiguration
{
    public string AuthServerUrl { get; set; } = string.Empty;
    public string Realm { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string AdminUsername { get; set; } = string.Empty;
    public string AdminPassword { get; set; } = string.Empty;
    public string TargetRealm { get; set; } = string.Empty;
}

public class IdentityService(
    HttpClient httpClient,
    IOptions<KeycloakAdminConfiguration> options)
{
    private readonly string _realm = options.Value.TargetRealm;
    private readonly HttpClient _httpClient = httpClient;

    // --- Users ---

    public async Task<ErrorOr<IEnumerable<KeycloakUser>>> GetUsersAsync(string? search = null)
    {
        var url = $"admin/realms/{_realm}/users";
        if (!string.IsNullOrEmpty(search)) url += $"?search={search}";
        
        var users = await _httpClient.GetFromJsonAsync<IEnumerable<UserRepresentation>>(url);
        
        return users?.Select(u => new KeycloakUser
        {
            Id = u.Id,
            Username = u.Username,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Email = u.Email,
            EmailVerified = u.EmailVerified ?? false,
            Enabled = u.Enabled ?? true
        }).ToList() ?? new List<KeycloakUser>();
    }

    public async Task<ErrorOr<string>> CreateUserAsync(KeycloakUser user)
    {
        var userRep = new UserRepresentation
        {
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Enabled = true,
            EmailVerified = true,
            Credentials = new List<CredentialRepresentation>
            {
                new() { Type = "password", Value = "0000", Temporary = true }
            }
        };

        var response = await _httpClient.PostAsJsonAsync($"admin/realms/{_realm}/users", userRep);
        
        if (response.IsSuccessStatusCode)
        {
            // Extract ID from Location header
            // Format: .../admin/realms/{realm}/users/{id}
            var location = response.Headers.Location;
            if (location != null)
            {
                var segments = location.AbsolutePath.Split('/');
                var userId = segments.Last();
                if (!string.IsNullOrEmpty(userId))
                {
                    return userId;
                }
            }
            
            // Fallback: If location header is somehow missing (unlikely in Keycloak),
            // we might need to query by username, but primarily we rely on Location.
             return Error.Failure(description: "User created but failed to retrieve ID.");
        }

        if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
             return Error.Conflict(description: "User already exists.");
        }
        
        return Error.Failure(description: $"Failed to create user: {response.StatusCode}");
    }

    public async Task<ErrorOr<Deleted>> DeleteUserAsync(string id)
    {
        var response = await _httpClient.DeleteAsync($"admin/realms/{_realm}/users/{id}");
        
        if (response.IsSuccessStatusCode)
        {
            return Result.Deleted;
        }

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Error.NotFound(description: "User not found.");
        }

        return Error.Failure(description: $"Failed to delete user: {response.StatusCode}");
    }

    // --- Roles ---

    public async Task<ErrorOr<IEnumerable<KeycloakRole>>> GetRolesAsync()
    {
        var roles = await _httpClient.GetFromJsonAsync<IEnumerable<RoleRepresentation>>($"admin/realms/{_realm}/roles");
        
        return roles?.Select(r => new KeycloakRole
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description
        }).ToList() ?? new List<KeycloakRole>();
    }

    public async Task<ErrorOr<Created>> CreateRoleAsync(KeycloakRole role)
    {
        var roleRep = new RoleRepresentation
        {
            Name = role.Name,
            Description = role.Description
        };

        var response = await _httpClient.PostAsJsonAsync($"admin/realms/{_realm}/roles", roleRep);
        
        if (response.IsSuccessStatusCode)
        {
            return Result.Created;
        }
        
        if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
             return Error.Conflict(description: "Role already exists.");
        }

        return Error.Failure(description: $"Failed to create role: {response.StatusCode}");
    }

    public async Task<ErrorOr<Deleted>> DeleteRoleAsync(string name)
    {
        var response = await _httpClient.DeleteAsync($"admin/realms/{_realm}/roles/{name}");
        
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
        RoleRepresentation? role = null;
        try 
        {
             role = await _httpClient.GetFromJsonAsync<RoleRepresentation>($"admin/realms/{_realm}/roles/{roleName}");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
             return Error.NotFound(description: $"Role '{roleName}' not found.");
        }

        if (role == null) return Error.NotFound(description: $"Role '{roleName}' not found.");

        var response = await _httpClient.PostAsJsonAsync($"admin/realms/{_realm}/users/{userId}/role-mappings/realm", new[] { role });
        
        if (response.IsSuccessStatusCode)
        {
            return Result.Success;
        }

        return Error.Failure(description: $"Failed to assign role: {response.StatusCode}");
    }

    public async Task<ErrorOr<Success>> RemoveRoleFromUserAsync(string userId, string roleName)
    {
        RoleRepresentation? role = null;
        try 
        {
             role = await _httpClient.GetFromJsonAsync<RoleRepresentation>($"admin/realms/{_realm}/roles/{roleName}");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
             return Error.NotFound(description: $"Role '{roleName}' not found.");
        }

        if (role == null) return Error.NotFound(description: $"Role '{roleName}' not found.");

        var request = new HttpRequestMessage(HttpMethod.Delete, $"admin/realms/{_realm}/users/{userId}/role-mappings/realm")
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
        var roles = await _httpClient.GetFromJsonAsync<IEnumerable<RoleRepresentation>>($"admin/realms/{_realm}/users/{userId}/role-mappings/realm");
        
        return roles?.Select(r => new KeycloakRole
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description
        }).ToList() ?? new List<KeycloakRole>();
    }
    
    // Update User
    public async Task<ErrorOr<Success>> UpdateUserAsync(string id, KeycloakUser user)
    {
         var userRep = new UserRepresentation
        {
            Id = id,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Enabled = user.Enabled,
            EmailVerified = user.EmailVerified
        };

        var response = await _httpClient.PutAsJsonAsync($"admin/realms/{_realm}/users/{id}", userRep);
        
        if (response.IsSuccessStatusCode)
        {
            return Result.Success;
        }
        
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
             return Error.NotFound(description: "User not found.");
        }
        
        return Error.Failure(description: $"Failed to update user: {response.StatusCode}");
    }
}

