using FastEndpoints;
using AspireAppTemplate.ApiService.Services;
using AspireAppTemplate.Shared;
using AspireAppTemplate.ApiService.Infrastructure.Extensions;

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
        var result = await identityService.RemoveRoleFromUserAsync(req.Id, req.RoleName);
        await this.SendResultAsync(result);
    }
}
