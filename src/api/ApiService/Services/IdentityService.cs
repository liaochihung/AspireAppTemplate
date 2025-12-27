using Keycloak.Net;
using Keycloak.Net.Models.Roles;
using Keycloak.Net.Models.Users;
using Microsoft.Extensions.Options;

namespace AspireAppTemplate.ApiService.Services;

public class KeycloakConfiguration
{
    public string BaseUrl { get; set; } = string.Empty;
    public string Realm { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}

public class IdentityService
{
    private readonly KeycloakClient _client;
    private readonly string _realm;

    public IdentityService(IOptions<KeycloakConfiguration> options)
    {
        var config = options.Value;
        _realm = config.Realm;
        // Keycloak.Net configuration for Service Account
        var kcOptions = new Keycloak.Net.Models.Clients.KeycloakOptions
        {
            AdminClientId = config.ClientId,
        };
        
        _client = new KeycloakClient(config.BaseUrl, config.ClientSecret, kcOptions);
    }

    // --- Users ---

    public Task<IEnumerable<User>> GetUsersAsync(string? search = null)
    {
        return _client.GetUsersAsync(_realm, search: search);
    }

    public Task<User> GetUserAsync(string id)
    {
        return _client.GetUserAsync(_realm, id);
    }

    public Task<bool> CreateUserAsync(User user)
    {
        return _client.CreateUserAsync(_realm, user);
    }

    public Task<bool> UpdateUserAsync(string id, User user)
    {
        return _client.UpdateUserAsync(_realm, id, user);
    }

    public Task<bool> DeleteUserAsync(string id)
    {
        return _client.DeleteUserAsync(_realm, id);
    }

    // --- Roles ---

    public Task<IEnumerable<Role>> GetRolesAsync()
    {
        return _client.GetRolesAsync(_realm);
    }

    public Task<bool> CreateRoleAsync(Role role)
    {
        return _client.CreateRoleAsync(_realm, role);
    }

    public Task<bool> DeleteRoleAsync(string name)
    {
        return _client.DeleteRoleByNameAsync(_realm, name);
    }

    // --- Mappings ---

    public async Task<bool> AssignRoleTokenUserAsync(string userId, string roleName)
    {
        var role = await _client.GetRoleAsync(_realm, roleName);
        if (role == null) return false;

        return await _client.AddRealmRoleMappingsToUserAsync(_realm, userId, new[] { role });
    }

    public async Task<bool> RemoveRoleFromUserAsync(string userId, string roleName)
    {
        var role = await _client.GetRoleAsync(_realm, roleName);
        if (role == null) return false;

        return await _client.DeleteRealmRoleMappingsFromUserAsync(_realm, userId, new[] { role });
    }
}
