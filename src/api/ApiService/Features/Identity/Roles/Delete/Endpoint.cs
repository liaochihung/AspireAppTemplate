using FastEndpoints;
using AspireAppTemplate.ApiService.Services;
using AspireAppTemplate.Shared;

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
        await identityService.DeleteRoleAsync(req.Name);
        await SendOkAsync(ct);
    }
}
