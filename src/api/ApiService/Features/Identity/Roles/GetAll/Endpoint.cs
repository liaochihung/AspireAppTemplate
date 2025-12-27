using FastEndpoints;
using AspireAppTemplate.ApiService.Services;
using AspireAppTemplate.Shared;
using AspireAppTemplate.ApiService.Infrastructure.Extensions;

namespace AspireAppTemplate.ApiService.Features.Identity.Roles.GetAll;

public class Endpoint(IdentityService identityService) : EndpointWithoutRequest<IEnumerable<KeycloakRole>>
{
    public override void Configure()
    {
        Get("/roles");
        Policies(AppPolicies.CanManageRoles);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await identityService.GetRolesAsync();
        await this.SendResultAsync(result, ct: ct);
    }
}
