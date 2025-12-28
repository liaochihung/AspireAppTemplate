using FastEndpoints;
using AspireAppTemplate.ApiService.Services;
using AspireAppTemplate.Shared;
using AspireAppTemplate.ApiService.Infrastructure.Extensions;
using Microsoft.AspNetCore.OutputCaching;

namespace AspireAppTemplate.ApiService.Features.Identity.Users.GetAll;

public class Endpoint(IdentityService identityService) : EndpointWithoutRequest<IEnumerable<KeycloakUser>>
{
    public override void Configure()
    {
        Get("/users");
        Policies(AppPolicies.CanManageUsers);
        Options(x => x.CacheOutput(c => c.Expire(TimeSpan.FromMinutes(5)).Tag("users")));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await identityService.GetUsersAsync();
        await this.SendResultAsync(result);
    }
}
