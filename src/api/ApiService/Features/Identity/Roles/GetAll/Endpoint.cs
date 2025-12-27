using FastEndpoints;
using AspireAppTemplate.ApiService.Services;
using AspireAppTemplate.Shared;

namespace AspireAppTemplate.ApiService.Features.Identity.Roles.GetAll;

public class Endpoint(IdentityService identityService) : EndpointWithoutRequest<IEnumerable<KeycloakRole>>
{
    public override void Configure()
    {
        Get("/identity/roles");
        Policies(AppPolicies.CanManageRoles);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var roles = await identityService.GetRolesAsync();
        await SendOkAsync(roles, ct);
    }
}
