using FastEndpoints;
using AspireAppTemplate.ApiService.Services;
using AspireAppTemplate.Shared;
using AspireAppTemplate.ApiService.Infrastructure.Extensions;
using Microsoft.AspNetCore.OutputCaching;

namespace AspireAppTemplate.ApiService.Features.Identity.Roles.GetAll;

public class Endpoint(IdentityService identityService) : Endpoint<PaginationRequest, PaginatedResult<KeycloakRole>>
{
    public override void Configure()
    {
        Get("/roles");
        Policies(AppPolicies.CanManageRoles);
        Version(1);
        Options(x => x.CacheOutput(c => c.Expire(TimeSpan.FromMinutes(10)).Tag("roles")));
    }

    public override async Task HandleAsync(PaginationRequest req, CancellationToken ct)
    {
        var result = await identityService.GetRolesAsync();
        
        if (result.IsError)
        {
            await SendAsync(new PaginatedResult<KeycloakRole>(), cancellation: ct);
            return;
        }

        var roles = result.Value;
        
        // Apply search filter
        if (!string.IsNullOrWhiteSpace(req.SearchTerm))
        {
            var term = req.SearchTerm.ToLower();
            roles = roles.Where(r => 
                (r.Name?.ToLower().Contains(term) ?? false) ||
                (r.Description?.ToLower().Contains(term) ?? false));
        }

        var paginated = roles.ToPaginatedResult(req);
        await SendAsync(paginated, cancellation: ct);
    }
}
