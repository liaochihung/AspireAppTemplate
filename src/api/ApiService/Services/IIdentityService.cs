using AspireAppTemplate.Shared;
using ErrorOr;
using Keycloak.AuthServices.Sdk.Admin.Models;

namespace AspireAppTemplate.ApiService.Services;

public interface IIdentityService
{
    Task<ErrorOr<IEnumerable<KeycloakUser>>> GetUsersAsync(string? search = null);
    Task<ErrorOr<Created>> CreateUserAsync(KeycloakUser user);
    Task<ErrorOr<Deleted>> DeleteUserAsync(string id);
    Task<ErrorOr<Success>> UpdateUserAsync(string id, KeycloakUser user);
    
    Task<ErrorOr<IEnumerable<KeycloakRole>>> GetRolesAsync();
    Task<ErrorOr<Created>> CreateRoleAsync(KeycloakRole role);
    Task<ErrorOr<Deleted>> DeleteRoleAsync(string name);
    
    Task<ErrorOr<Success>> AssignRoleToUserAsync(string userId, string roleName);
    Task<ErrorOr<Success>> RemoveRoleFromUserAsync(string userId, string roleName);
    Task<ErrorOr<IEnumerable<KeycloakRole>>> GetUserRolesAsync(string userId);
}
