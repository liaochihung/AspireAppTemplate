using FastEndpoints;
using AspireAppTemplate.ApiService.Services;
using AspireAppTemplate.Shared;
using AspireAppTemplate.ApiService.Infrastructure.Extensions;
using Microsoft.AspNetCore.OutputCaching;

namespace AspireAppTemplate.ApiService.Features.Identity.Users.AssignRole;

public class UserRoleRequest
{
    public string Id { get; set; } = string.Empty; // From Route
    public string RoleName { get; set; } = string.Empty; // From Body
}

public class Endpoint(IIdentityService identityService, IOutputCacheStore cacheStore) : Endpoint<UserRoleRequest>
{
    public override void Configure()
    {
        Post("/users/{id}/roles");
        Policies(AppPolicies.CanManageUsers);
    }

    public override async Task HandleAsync(UserRoleRequest req, CancellationToken ct)
    {
        var result = await identityService.AssignRoleToUserAsync(req.Id, req.RoleName);
        await cacheStore.EvictByTagAsync("users", ct);
        await this.SendResultAsync(result, ct: ct);
    }
}
