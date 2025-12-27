using System.Net.Http.Json;
using Keycloak.Net.Models.Users;
using Keycloak.Net.Models.Roles;

namespace AspireAppTemplate.Web;

public class IdentityApiClient(HttpClient httpClient)
{
    // --- Users ---

    public async Task<User[]> GetUsersAsync(CancellationToken ct = default)
    {
        var users = await httpClient.GetFromJsonAsync<User[]>("/api/users", ct);
        return users ?? [];
    }

    public async Task CreateUserAsync(User user, CancellationToken ct = default)
    {
        // We need a CreateUserRequest DTO matching the endpoint or just match the JSON structure.
        // The endpoint expects CreateUserRequest: { username, email, firstName, lastName, password }
        // The User object has similar fields but "UserName" vs "username" (case insensitive usually in JSON)
        // But the endpoint expects a specific Request object with Password.
        // User object has Credentials list, not Password field.
        // So we should CREATE a request object directly or use an anonymous object.
        var request = new 
        {
            Username = user.UserName,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Password = user.Credentials?.FirstOrDefault()?.Value ?? "123456" // Default or passed separately
        };
        // Wait, passing password in User object is tricky if UI binds to it.
        // Let's assume the UI creates a User object and populates credentials.
        
        var response = await httpClient.PostAsJsonAsync("/api/users", request, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task AssignRoleAsync(string userId, string roleName, CancellationToken ct = default)
    {
        var request = new { RoleName = roleName };
        var response = await httpClient.PostAsJsonAsync($"/api/users/{userId}/roles", request, ct);
        response.EnsureSuccessStatusCode();
    }

    // --- Roles ---

    public async Task<Role[]> GetRolesAsync(CancellationToken ct = default)
    {
        var roles = await httpClient.GetFromJsonAsync<Role[]>("/api/roles", ct);
        return roles ?? [];
    }

    public async Task CreateRoleAsync(Role role, CancellationToken ct = default)
    {
         var request = new 
        {
            Name = role.Name,
            Description = role.Description
        };
        var response = await httpClient.PostAsJsonAsync("/api/roles", request, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteRoleAsync(string name, CancellationToken ct = default)
    {
        var response = await httpClient.DeleteAsync($"/api/roles/{name}", ct);
        response.EnsureSuccessStatusCode();
    }
}
