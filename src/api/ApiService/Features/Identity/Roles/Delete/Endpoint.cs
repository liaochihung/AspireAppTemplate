using FastEndpoints;
using AspireAppTemplate.ApiService.Services;
using AspireAppTemplate.Shared;
using AspireAppTemplate.ApiService.Infrastructure.Extensions;

namespace AspireAppTemplate.ApiService.Features.Identity.Roles.Delete;

public class DeleteRoleRequest
{
    public string Name { get; set; } = default!;
}

public class Endpoint(IdentityService identityService) : Endpoint<DeleteRoleRequest>
{
    public override void Configure()
    {
        Delete("/identity/roles/{Name}");
        Policies(AppPolicies.CanManageRoles);
    }

    public override async Task HandleAsync(DeleteRoleRequest req, CancellationToken ct)
    {
        var result = await identityService.DeleteRoleAsync(req.Name);
        await this.SendResultAsync(result, ct: ct);
    }
}
