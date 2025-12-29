using FastEndpoints;
using AspireAppTemplate.ApiService.Services;
using AspireAppTemplate.Shared;
using AspireAppTemplate.ApiService.Infrastructure.Extensions;
using Microsoft.AspNetCore.OutputCaching;

using AspireAppTemplate.ApiService.Infrastructure.Services;

namespace AspireAppTemplate.ApiService.Features.Identity.Users.RemoveRole;

public class RemoveUserRoleRequest
{
    public string Id { get; set; } = string.Empty; // User Id
    public string RoleName { get; set; } = string.Empty;
}

public class Endpoint(IdentityService identityService, IOutputCacheStore cacheStore, IAuditService auditService) : Endpoint<RemoveUserRoleRequest>
{
    public override void Configure()
    {
        Delete("/users/{id}/roles/{RoleName}");
        Policies(AppPolicies.CanManageUsers);
    }

    public override async Task HandleAsync(RemoveUserRoleRequest req, CancellationToken ct)
    {
        var result = await identityService.RemoveRoleFromUserAsync(req.Id, req.RoleName);
        
        if (!result.IsError)
        {
             await auditService.LogAsync("RemoveRole", "User", req.Id, new { Role = req.RoleName }, null, ct);
        }

        await cacheStore.EvictByTagAsync("users", ct);
        await this.SendResultAsync(result, ct: ct);
    }
}
