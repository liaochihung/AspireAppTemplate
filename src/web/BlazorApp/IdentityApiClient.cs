using System.Net.Http.Json;
using System.Web;
using AspireAppTemplate.Shared;

namespace AspireAppTemplate.Web;

public class IdentityApiClient(HttpClient httpClient)
{
    // --- Users ---

    public async Task<KeycloakUser[]> GetUsersAsync(CancellationToken ct = default)
    {
        var result = await GetUsersPaginatedAsync(new PaginationRequest { Page = 1, PageSize = 1000 }, ct);
        return result.Items.ToArray();
    }

    public async Task<PaginatedResult<KeycloakUser>> GetUsersPaginatedAsync(PaginationRequest request, CancellationToken ct = default)
    {
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["page"] = request.Page.ToString();
        query["pageSize"] = request.PageSize.ToString();
        if (!string.IsNullOrEmpty(request.SearchTerm))
            query["searchTerm"] = request.SearchTerm;

        var result = await httpClient.GetFromJsonAsync<PaginatedResult<KeycloakUser>>($"/api/users?{query}", ct);
        return result ?? new PaginatedResult<KeycloakUser>();
    }

    public async Task CreateUserAsync(KeycloakUser user, CancellationToken ct = default)
    {
        var request = new 
        {
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Password = user.Credentials?.FirstOrDefault()?.Value ?? "123456"
        };
        
        var response = await httpClient.PostAsJsonAsync("/api/users", request, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteUserAsync(string id, CancellationToken ct = default)
    {
        var response = await httpClient.DeleteAsync($"/api/users/{id}", ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateUserAsync(KeycloakUser user, CancellationToken ct = default)
    {
        var request = new 
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Enabled = user.Enabled
        };
        var response = await httpClient.PutAsJsonAsync($"/api/users/{user.Id}", request, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task AssignRoleAsync(string userId, string roleName, CancellationToken ct = default)
    {
        var request = new { RoleName = roleName };
        var response = await httpClient.PostAsJsonAsync($"/api/users/{userId}/roles", request, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveRoleAsync(string userId, string roleName, CancellationToken ct = default)
    {
        var response = await httpClient.DeleteAsync($"/api/users/{userId}/roles/{roleName}", ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task<KeycloakRole[]> GetUserRolesAsync(string userId, CancellationToken ct = default)
    {
        var roles = await httpClient.GetFromJsonAsync<KeycloakRole[]>($"/api/users/{userId}/roles", ct);
        return roles ?? [];
    }

    // --- Roles ---

    public async Task<KeycloakRole[]> GetRolesAsync(CancellationToken ct = default)
    {
        var result = await GetRolesPaginatedAsync(new PaginationRequest { Page = 1, PageSize = 1000 }, ct);
        return result.Items.ToArray();
    }

    public async Task<PaginatedResult<KeycloakRole>> GetRolesPaginatedAsync(PaginationRequest request, CancellationToken ct = default)
    {
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["page"] = request.Page.ToString();
        query["pageSize"] = request.PageSize.ToString();
        if (!string.IsNullOrEmpty(request.SearchTerm))
            query["searchTerm"] = request.SearchTerm;

        var result = await httpClient.GetFromJsonAsync<PaginatedResult<KeycloakRole>>($"/api/roles?{query}", ct);
        return result ?? new PaginatedResult<KeycloakRole>();
    }

    public async Task CreateRoleAsync(KeycloakRole role, CancellationToken ct = default)
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
