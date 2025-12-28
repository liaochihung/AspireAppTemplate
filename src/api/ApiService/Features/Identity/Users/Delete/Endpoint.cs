using AspireAppTemplate.ApiService.Infrastructure.Extensions;
using AspireAppTemplate.ApiService.Services;
using AspireAppTemplate.Shared;
using FastEndpoints;
using Microsoft.AspNetCore.OutputCaching;

namespace AspireAppTemplate.ApiService.Features.Identity.Users.Delete;

public class Endpoint(IdentityService identityService, IOutputCacheStore cacheStore) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/users/{id}");
        Policies(AppPolicies.CanManageUsers);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<string>("id");
        if (string.IsNullOrEmpty(id))
        {
            await SendErrorsAsync(400, ct);
            return;
        }

        var result = await identityService.DeleteUserAsync(id);
        await cacheStore.EvictByTagAsync("users", ct);
        await this.SendResultAsync(result);
    }
}
