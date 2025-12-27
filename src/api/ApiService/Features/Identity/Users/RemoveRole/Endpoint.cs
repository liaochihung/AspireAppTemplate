using FastEndpoints;
using AspireAppTemplate.ApiService.Services;
using AspireAppTemplate.Shared;

namespace AspireAppTemplate.ApiService.Features.Identity.Users.RemoveRole;

public class RemoveUserRoleRequest
{
    public string Id { get; set; } = string.Empty; // User Id
    public string RoleName { get; set; } = string.Empty;
}

public class Endpoint(IdentityService identityService) : Endpoint<RemoveUserRoleRequest>
{
    public override void Configure()
    {
        Delete("/users/{id}/roles/{RoleName}");
        Policies(AppPolicies.CanManageUsers);
    }

    public override async Task HandleAsync(RemoveUserRoleRequest req, CancellationToken ct)
    {
        var success = await identityService.RemoveRoleFromUserAsync(req.Id, req.RoleName);
        if (success)
            await SendOkAsync(ct);
        else
        {
            AddError("Failed to remove role");
            await SendErrorsAsync(cancellation: ct);
        }
    }
}
