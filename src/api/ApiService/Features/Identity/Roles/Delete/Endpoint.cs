using FastEndpoints;
using AspireAppTemplate.ApiService.Services;
using AspireAppTemplate.Shared;
using AspireAppTemplate.ApiService.Infrastructure.Extensions;
using Microsoft.AspNetCore.OutputCaching;

using AspireAppTemplate.ApiService.Infrastructure.Services;

namespace AspireAppTemplate.ApiService.Features.Identity.Roles.Delete;

public class DeleteRoleRequest
{
    public string Name { get; set; } = default!;
}

public class Endpoint(IdentityService identityService, IOutputCacheStore cacheStore, IAuditService auditService) : Endpoint<DeleteRoleRequest>
{
    public override void Configure()
    {
        Delete("/roles/{Name}");
        Policies(AppPolicies.CanManageRoles);
        Version(1);
    }

    public override async Task HandleAsync(DeleteRoleRequest req, CancellationToken ct)
    {
        var result = await identityService.DeleteRoleAsync(req.Name);
        
        if (!result.IsError)
        {
             await auditService.LogAsync("Delete", "Role", req.Name, null, null, ct);
        }

        await cacheStore.EvictByTagAsync("roles", ct);
        await this.SendResultAsync(result, ct: ct);
    }
}
