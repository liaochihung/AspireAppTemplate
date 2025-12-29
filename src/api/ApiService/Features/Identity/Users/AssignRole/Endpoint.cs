using FastEndpoints;
using AspireAppTemplate.ApiService.Services;
using AspireAppTemplate.Shared;
using AspireAppTemplate.ApiService.Infrastructure.Extensions;
using Microsoft.AspNetCore.OutputCaching;

using AspireAppTemplate.ApiService.Infrastructure.Services;

namespace AspireAppTemplate.ApiService.Features.Identity.Users.AssignRole;

public class UserRoleRequest
{
    public string Id { get; set; } = string.Empty; // From Route
    public string RoleName { get; set; } = string.Empty; // From Body
}

public class Endpoint(IdentityService identityService, IOutputCacheStore cacheStore, IAuditService auditService) : Endpoint<UserRoleRequest>
{
    public override void Configure()
    {
        Post("/users/{id}/roles");
        Policies(AppPolicies.CanManageUsers);
    }

    public override async Task HandleAsync(UserRoleRequest req, CancellationToken ct)
    {
        var result = await identityService.AssignRoleToUserAsync(req.Id, req.RoleName);
        
        if (!result.IsError)
        {
             await auditService.LogAsync("AssignRole", "User", req.Id, null, new { Role = req.RoleName }, ct);
        }

        await cacheStore.EvictByTagAsync("users", ct);
        await this.SendResultAsync(result, ct: ct);
    }
}
