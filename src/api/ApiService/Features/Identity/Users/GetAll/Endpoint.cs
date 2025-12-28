using FastEndpoints;
using AspireAppTemplate.ApiService.Services;
using AspireAppTemplate.Shared;
using AspireAppTemplate.ApiService.Infrastructure.Extensions;
using Microsoft.AspNetCore.OutputCaching;

namespace AspireAppTemplate.ApiService.Features.Identity.Users.GetAll;

public class Endpoint(IdentityService identityService) : Endpoint<PaginationRequest, PaginatedResult<KeycloakUser>>
{
    public override void Configure()
    {
        Get("/users");
        Policies(AppPolicies.CanManageUsers);
        Options(x => x.CacheOutput(c => c.Expire(TimeSpan.FromMinutes(5)).Tag("users")));
    }

    public override async Task HandleAsync(PaginationRequest req, CancellationToken ct)
    {
        var result = await identityService.GetUsersAsync(req.SearchTerm);
        
        if (result.IsError)
        {
            await SendAsync(new PaginatedResult<KeycloakUser>(), cancellation: ct);
            return;
        }

        var paginated = result.Value.ToPaginatedResult(req);
        await SendAsync(paginated, cancellation: ct);
    }
}
