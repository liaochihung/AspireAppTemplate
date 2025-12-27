using FastEndpoints;
using AspireAppTemplate.ApiService.Services;
using AspireAppTemplate.Shared;
using AspireAppTemplate.ApiService.Infrastructure.Extensions;

namespace AspireAppTemplate.ApiService.Features.Identity.Users.GetRoles;

public class GetUserRolesRequest
{
    public string Id { get; set; } = string.Empty;
}

public class Endpoint(IdentityService identityService) : Endpoint<GetUserRolesRequest, IEnumerable<KeycloakRole>>
{
    public override void Configure()
    {
        Get("/users/{id}/roles");
        Policies(AppPolicies.CanManageUsers);
    }

    public override async Task HandleAsync(GetUserRolesRequest req, CancellationToken ct)
    {
        var result = await identityService.GetUserRolesAsync(req.Id);
        await this.SendResultAsync(result, ct: ct);
    }
}
