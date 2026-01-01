using AspireAppTemplate.ApiService.Infrastructure.Extensions;
using AspireAppTemplate.ApiService.Services;
using AspireAppTemplate.Shared;
using FastEndpoints;
using Microsoft.AspNetCore.OutputCaching;

using AspireAppTemplate.ApiService.Infrastructure.Services;

namespace AspireAppTemplate.ApiService.Features.Identity.Users.Delete;

public class Endpoint(IdentityService identityService, IOutputCacheStore cacheStore, IAuditService auditService) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/users/{id}");
        Policies(AppPolicies.CanManageUsers);
        Version(1);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<string>("id");
        if (string.IsNullOrEmpty(id))
        {
            await SendErrorsAsync(400, ct);
            return;
        }

        // Prevent self-deletion
        var currentUserId = User.FindFirst("sub")?.Value;
        if (currentUserId == id)
        {
            AddError("Cannot delete your own account.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        var result = await identityService.DeleteUserAsync(id);
        
        if (!result.IsError)
        {
            await auditService.LogAsync("Delete", "User", id, null, null, ct);
        }

        await cacheStore.EvictByTagAsync("users", ct);
        await this.SendResultAsync(result);
    }
}
